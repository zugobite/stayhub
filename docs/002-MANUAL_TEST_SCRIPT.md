# StayHub Manual Test Script

Walk this script end to end before submitting. It exercises every row of the grading rubric in order, and each step names the table you can query to prove the behaviour persisted.

## Prerequisites

1. SQL Server is running (`docker compose up -d`).
2. The application is running (`dotnet run --project src/StayHub.Web`).
3. Browse to `https://localhost:7244`.
4. The first start applies the migrations and seeds roles, the SuperAdmin, and demonstration data.

### Seeded accounts

| Role       | Email                       | Password         |
| ---------- | --------------------------- | ---------------- |
| SuperAdmin | `superadmin@stayhub.co.za`  | `SuperAdmin123!` |
| Admin      | `admin@stayhub.co.za`       | `Password123!`   |
| Host       | `host@stayhub.co.za`        | `Password123!`   |
| Guest      | `guest@stayhub.co.za`       | `Password123!`   |

---

## Test Scenario 1: Registration and the Guest role

### Step 1: Register

1. Click **Sign up**.
2. Enter a first name, last name, a new email, and a password of at least 8 characters including a non-alphanumeric.
3. Submit.

**Expected:** You are signed in and returned to the landing page. The header shows **Become a Host** and **Host Application Status**, which only Guests see.

**Verify in SQL:** query 1 in `sql/003-verification-queries.sql` - the new user holds exactly one role, `Guest`.

### Step 2: Confirm the login was logged

Sign out, then sign back in.

**Verify in SQL:** `SELECT TOP 5 * FROM ActivityLogs ORDER BY CreatedAt DESC;` shows `UserLoggedIn` with `ActivityType = 'Login'`.

---

## Test Scenario 2: Host application (image rule)

### Step 3: Try to apply without a photo

1. As the Guest, click **Become a Host**.
2. Fill in every field but attach **no** image.
3. Submit.

**Expected:** The form is rejected with "At least one property photo is required." Nothing is written to `Properties` or `HostApplications`.

### Step 4: Apply correctly

1. Attach one or more images.
2. Submit.

**Expected:** Redirected to **Host Application Status** showing the application as `Pending`.

**Verify in SQL:** queries 3 and 4 - the application exists, and its property has `ImageCount >= 1`. The property's `IsActive` is `0`, because it is not published until approval.

---

## Test Scenario 3: Admin review and the role swap

### Step 5: Review the application

1. Sign out, sign in as `admin@stayhub.co.za`.
2. You land on **/Admin/Dashboard**. Confirm the stat cards populate.
3. Open **Host Requests**, click **Review** on the pending application.
4. Confirm the applicant, listing details, and photos render.
5. Click **Approve application**.

**Expected:** The queue shows the application as `Approved`.

**Verify in SQL:**
- Query 2 - the applicant now holds `Host` and **not** `Guest`.
- `SELECT * FROM ActivityLogs WHERE ActivityType = 'RoleChange';` contains `RoleChangedGuestToHost`.
- The property's `IsActive` is now `1`.

### Step 6: Confirm the notification reached the applicant

Sign in as the promoted user and open **Notifications**.

**Expected:** "Host Application Approved" appears, styled as unread. Clicking it moves it to READ.

**Verify in SQL:** query 6 - the notification's `IsRead` flipped to `1`.

### Step 7: Role isolation

While signed in as the new Host, browse directly to `/Bookings`.

**Expected:** You are sent to **Access Denied** - that route is Guest-only. Likewise `/Host/Dashboard` is refused for a Guest account.

---

## Test Scenario 4: Booking engine and pricing

### Step 8: Book a stay

1. Sign in as `guest@stayhub.co.za`.
2. Open **Properties**, filter by city, then open **Lovely City Apartment** (R 780 per person / night, R 350 cleaning, 12% service fee).
3. Set check-in and checkout dates, then change the **Guests** field.

**Expected:** Rates are quoted **per person, per night**, and the breakdown updates live as either the dates or the guest count changes, matching:

```
TotalPrice = (PricePerNight × Nights × Guests) + CleaningFee + ServiceFee
```

Worked example for 2 nights and 3 guests:

| Line | Amount |
| ---- | ------ |
| R 780 × 2 nights × 3 guests | R 4 680,00 |
| Cleaning fee | R 350,00 |
| Service fee (12% of R 4 680) | R 561,60 |
| **Total** | **R 5 591,60** |

The cleaning fee is flat per stay; the service fee scales with the subtotal, so it grows with party size.

4. Click **Reserve**.

**Expected:** Redirected to **My trips** with the booking marked `Pending`.

**Verify in SQL:** query 5 - the booking exists with the total the widget displayed, computed server-side.

### Step 9: Host approves the booking

1. Sign in as the Host.
2. Open **Booking Requests** and click **Approve**.

**Expected:** The status becomes `Approved`.

**Verify in SQL:** `ActivityLogs` contains `BookingApproved` with `ActivityType = 'BookingStatusUpdate'`.

### Step 9b: Guest pays (simulated)

1. Sign in as the Guest.
2. Open **My trips**.
3. On the approved booking, open the actions menu and click **Pay Now**.

**Expected:** You leave the StayHub pages for the **StayPay Sandbox** checkout — a dark header with a `SANDBOX` badge, an order summary repeating the per-person breakdown, and the card form already filled in with `4242 4242 4242 4242`, expiry `12/30`, CVV `123`.

#### 9b-i: A declined card

4. Replace the card number with `4000 0000 0000 0002` and submit.

**Expected:** The page returns with *"Charge declined: declined by issuer."* The booking stays `Approved` so you can try again.

**Verify in SQL:** `SELECT Status, CardLast4, FailureReason FROM Payments WHERE BookingId = <id>;` shows `Failed`, `0002`, and the reason.

#### 9b-ii: An approved card

5. Restore `4242 4242 4242 4242` and submit.

**Expected:** A receipt page showing *Payment successful*, a reference such as `SP-QDDV4S2S`, and the card masked to its last four digits. No money moves — the page says so.

**Verify in SQL:**
- The booking's `Status` is `Paid`.
- `SELECT * FROM Payments WHERE BookingId = <id>;` shows one row (the declined attempt was updated, not duplicated), `Status = 'Paid'`, and a populated `Reference`.
- `ActivityLogs` contains both `PaymentDeclined` and `BookingPaid`.

---

## Test Scenario 5: Check-in workflow

### Step 10: Host defines the check-in process

1. As the Host, open **My Properties** → **Check-in Process** on the booked listing.
2. Enter a title and two or three steps, one per line.
3. Save.

**Verify in SQL:** `SELECT * FROM CheckInProcesses;` - `StepsJson` holds a JSON array of your steps.

### Step 11: Guest completes check-in

1. Sign in as the Guest, open **My trips**, click **Start Check-in** (available for `Approved` or `Paid`).
2. Work through each step.
3. On the final step, choose a document type and upload a JPG, PNG, or PDF.

**Expected:** Redirected to **My trips** with a confirmation that the document awaits verification.

**Verify in SQL:** `SELECT * FROM GuestCheckIns;` shows `Status = 'PendingVerification'`, and `GuestDocuments` holds the upload with `Status = 'Pending'`.

### Step 12: Host verifies the document

1. Sign in as the Host, open **Guest Check-ins**.
2. Open the document link to confirm the file was stored, then click **Verify**.

**Expected:** The queue empties.

**Verify in SQL:** query 5 - the booking's status is now `CheckedIn`. `GuestDocuments.Status` is `Verified`.

### Step 12b: Guest marks the stay complete and leaves a review

1. Sign in as the Guest, open **My trips**.
2. On the checked-in booking, click **Mark stay complete** and confirm.

**Expected:** Status becomes `Completed`.

3. Click **Leave a review**, choose a rating, write at least 10 characters, and submit.

**Expected:** Redirected to **My trips** with a success message. The listing's detail page shows the new review.

**Verify in SQL:**
- Booking `Status = 'Completed'`.
- `SELECT * FROM Reviews WHERE PropertyId = <id> AND ReviewerId = '<guestId>';` returns the new row.
- `ActivityLogs` contains `BookingCompleted` and `ReviewSubmitted`.

---

## Test Scenario 5b: Host edits a listing

### Step 12c: Edit Listing details

1. Sign in as `host@stayhub.co.za`.
2. Open **My Properties**, open the actions menu on a listing, click **Edit Listing**.
3. Change the title (or nightly price), optionally attach another photo, and save.

**Expected:** Redirected to **My Properties** with a success message. The public listing page shows the new title/price.

**Verify in SQL:** `ActivityLogs` contains `PropertyUpdated` for that listing.

---

## Test Scenario 6: Messaging

### Step 13: Admin → user message

1. As the Admin, open **Users** and click **Message** beside any user.
2. Send a message.
3. Sign in as that user and open **Inbox**.

**Expected:** The conversation appears as a single thread row. Opening it marks messages read.

**Verify in SQL:** query 6 - the message's `IsRead` moved from `0` to `1` once the thread was opened.

### Step 13b: Guest ↔ Host live messaging

1. Sign in as `guest@stayhub.co.za` in one browser.
2. Open a listing and click **Message** under Meet your host (or open **My Trips** → **Message Host**).
3. Send a message.
4. In a second browser, sign in as `host@stayhub.co.za` **without refreshing** once the host is already on the site (or leave the conversation open).

**Expected:**
- The host's message badge increments live and a toast appears.
- If the host has the conversation open, the new bubble appears without a page reload.
- From **Booking Requests** or **Guest check-ins**, the host can click **Message Guest** and reply; the guest sees the reply live.

**Expected (AJAX):** Sending from the conversation page appends the bubble immediately; a full page reload is not required.

---

## Test Scenario 7: SuperAdmin promotion

### Step 14: Promote a Guest to Admin

1. Sign in as `superadmin@stayhub.co.za`.
2. Open **Users**. The **Promote to Admin** button is visible.
3. Promote a Guest.

**Expected:** The user's role chips show `Admin`.

**Verify in SQL:** `ActivityLogs` contains `RoleChangedGuestToAdmin`.

### Step 15: Confirm a plain Admin cannot promote

Sign in as `admin@stayhub.co.za` and open **Users**.

**Expected:** The promote button is absent and the page explains that only the SuperAdmin can promote. Posting to `/Admin/PromoteToAdmin` directly returns Access Denied.

---

## Test Scenario 8: Audit trail

### Step 16: Review the logs

As an Admin, open **Activity Logs** and filter by type.

**Expected:** Entries exist for at least `Login`, `RoleChange`, `ApplicationSubmitted`, `ApplicationReviewed`, and `BookingStatusUpdate`.

**Verify in SQL:** query 7 returns a row per activity type with non-zero counts.

---

---

## Test Scenario 9: Advanced search and regional discovery

### Step 17: Browse by province from the landing page

1. Sign out (or stay signed in — the gallery is public).
2. On the home page, scroll to **Browse by Province** and click a region card.

**Expected:** The gallery opens filtered to that province, and the heading reads "N stays in <Province>". The seed spans all nine provinces, so every card leads somewhere.

### Step 18: Compose several filters at once

1. Open **Properties**.
2. Type `lagoon` in the search box.

**Expected:** Two results — Knysna Lagoon House by title, and Umhlanga Beach House because its address is *3 Lagoon Drive*. The keyword matches title, description, city and address.

3. Clear the search. Open **More filters** and set **Max price** to 1500.

**Expected:** Only the listings at or below R 1 500 per person remain.

4. Tick **Pool**, then also tick **Air Conditioning**.

**Expected:** The count *narrows* with the second tick. Amenities are AND-matched — a listing must offer every ticked amenity, not merely one.

5. Change **Sort by** to *Price: low to high*, then *Top rated*.

**Expected:** Jeffreys Bay Surf Room (R 450) leads on price; Camps Bay Villa leads on rating. Listings with no reviews sort last rather than being treated as zero-rated.

6. Click **Clear all**.

**Expected:** All 15 listings return and the "Filters applied" strip disappears.

---

## Test Scenario 10: Table pagination

### Step 19: Page through the audit trail

1. Sign in as `admin@stayhub.co.za` and open **Activity Logs**.

**Expected:** At most **15 rows**, with a pager reading "Showing 1–15 of N log entries".

2. Click page **2**.

**Expected:** Rows 16–30, and the previous-page arrow becomes active.

3. Filter by an activity type, then page again.

**Expected:** The filter *survives* paging — the address bar keeps `?type=...&page=2`, and the total in the pager reflects the filtered count, not the whole table.

4. Hand-edit the address to `?page=99`.

**Expected:** You land on the last page, not an empty table. Page numbers are clamped into range.

> The same 15-row pager appears on Users, Host Requests, Bookings, Booking Requests, Guest Check-ins, Earnings, and Application Status. The activity log is paged in SQL rather than in memory, because it is the one table that grows without bound.

---

## Test Scenario 11: Profile management

### Step 20: Edit your own profile

1. Sign in as `host@stayhub.co.za`.
2. Open the account menu and choose **My Profile**.
3. Change your name, add a **location** and an **about you** introduction, and upload a **profile photo**.
4. Save.

**Expected:** A confirmation appears, and the "HOW OTHERS SEE YOU" panel updates to show the photo, name, location and introduction.

**Verify in SQL:** `SELECT FirstName, LastName, Location, Bio, ProfilePictureUrl FROM AspNetUsers WHERE Email = 'host@stayhub.co.za';` — all five columns reflect what you typed, and the picture URL points into `/uploads/avatars/`.

### Step 21: Confirm the photo travels

1. Open one of that host's listings.

**Expected:** The **Meet your host** card shows the uploaded photo, the new name, the location, and the introduction. The header avatar shows it too.

### Step 22: Reject a bad upload, then remove the photo

1. Return to **My Profile** and try to upload a `.txt` file.

**Expected:** *"That photo could not be used. Use a JPG, PNG, WEBP or GIF under 5 MB."* — and the rest of the form is not silently saved around it.

2. Tick **Remove my current photo** and save.

**Expected:** The generated portrait returns in place of the photo.

**Verify in SQL:** `ProfilePictureUrl` is `NULL`, and `ActivityLogs` contains `ProfileUpdated` entries with `ActivityType = 'ProfileUpdated'`.


## Verification Checklist

- [ ] Registration assigns the Guest role automatically
- [ ] Host application is blocked without an image
- [ ] Approval adds Host and removes Guest atomically
- [ ] Approval publishes the listing (`IsActive = 1`)
- [ ] Guest-only and Host-only routes each refuse the other role
- [ ] Rates are quoted per person per night, and the total matches `(PricePerNight × Nights × Guests) + CleaningFee + ServiceFee`
- [ ] Changing the guest count updates the live breakdown as well as the dates
- [ ] Booking moves Pending → Approved → Paid → CheckedIn → Completed
- [ ] Pay Now opens the StayPay Sandbox checkout, pre-filled with a test card
- [ ] The declining test card records `PaymentStatus.Failed` with a reason and leaves the booking `Approved`
- [ ] The approving test card creates one `Payments` row with a reference and last-4, and sets the booking to `Paid`
- [ ] Guest can leave a review after completing a stay
- [ ] Host can edit an owned listing (title/price/photos) and the change persists
- [ ] Check-in steps persist as JSON, document uploads persist as files plus rows
- [ ] Message and notification `IsRead` flags update in the database
- [ ] Guest can message a host from a property page; host can reply from booking/check-in screens
- [ ] New messages update the recipient's badge (and open conversation) live via SignalR
- [ ] Only the SuperAdmin can promote a Guest to Admin
- [ ] ActivityLogs cover login, role change, and booking status update
- [ ] Province cards on the home page deep-link into the filtered gallery
- [ ] Keyword, price, guest, amenity and sort filters compose, and amenities are AND-matched
- [ ] Every data table shows at most 15 rows, with filters surviving paging and out-of-range pages clamped
- [ ] A user can edit their own name, location, bio and photo, and the photo appears on listings and in the header
- [ ] A non-image profile upload is rejected with a message rather than failing silently
- [ ] The database can be dropped and rebuilt from migrations alone
