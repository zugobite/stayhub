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

**Expected:** You are sent to **Access denied** - that route is Guest-only. Likewise `/Host/Dashboard` is refused for a Guest account.

---

## Test Scenario 4: Booking engine and pricing

### Step 8: Book a stay

1. Sign in as `guest@stayhub.co.za`.
2. Open **Properties**, filter by city, then open a listing.
3. Set check-in and checkout dates.

**Expected:** The price breakdown updates live and matches `(PricePerNight × Nights) + CleaningFee + ServiceFee`. For a one-night stay at R 3 100 with a 12% service fee, the service fee reads R 372.

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
3. On the approved booking, open the actions menu and click **Pay now**. Confirm.

**Expected:** Status becomes `Paid`.

**Verify in SQL:**
- The booking's `Status` is `Paid`.
- `SELECT * FROM Payments WHERE BookingId = <id>;` shows `Amount` matching the booking total and `Status = 'Paid'`.
- `ActivityLogs` contains `BookingPaid`.

---

## Test Scenario 5: Check-in workflow

### Step 10: Host defines the check-in process

1. As the Host, open **My Properties** → **Check-in Process** on the booked listing.
2. Enter a title and two or three steps, one per line.
3. Save.

**Verify in SQL:** `SELECT * FROM CheckInProcesses;` - `StepsJson` holds a JSON array of your steps.

### Step 11: Guest completes check-in

1. Sign in as the Guest, open **My trips**, click **Start check-in** (available for `Approved` or `Paid`).
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

### Step 12c: Edit listing details

1. Sign in as `host@stayhub.co.za`.
2. Open **My Properties**, open the actions menu on a listing, click **Edit listing**.
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
2. Open a listing and click **Message** under Meet your host (or open **My Trips** → **Message host**).
3. Send a message.
4. In a second browser, sign in as `host@stayhub.co.za` **without refreshing** once the host is already on the site (or leave the conversation open).

**Expected:**
- The host's message badge increments live and a toast appears.
- If the host has the conversation open, the new bubble appears without a page reload.
- From **Booking Requests** or **Guest check-ins**, the host can click **Message guest** and reply; the guest sees the reply live.

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

**Expected:** The promote button is absent and the page explains that only the SuperAdmin can promote. Posting to `/Admin/PromoteToAdmin` directly returns Access denied.

---

## Test Scenario 8: Audit trail

### Step 16: Review the logs

As an Admin, open **Activity Logs** and filter by type.

**Expected:** Entries exist for at least `Login`, `RoleChange`, `ApplicationSubmitted`, `ApplicationReviewed`, and `BookingStatusUpdate`.

**Verify in SQL:** query 7 returns a row per activity type with non-zero counts.

---

## Verification Checklist

- [ ] Registration assigns the Guest role automatically
- [ ] Host application is blocked without an image
- [ ] Approval adds Host and removes Guest atomically
- [ ] Approval publishes the listing (`IsActive = 1`)
- [ ] Guest-only and Host-only routes each refuse the other role
- [ ] Booking total matches `(PricePerNight × Nights) + CleaningFee + ServiceFee`
- [ ] Booking moves Pending → Approved → Paid → CheckedIn → Completed
- [ ] Simulated Pay creates a `Payments` row and sets booking status to `Paid`
- [ ] Guest can leave a review after completing a stay
- [ ] Host can edit an owned listing (title/price/photos) and the change persists
- [ ] Check-in steps persist as JSON, document uploads persist as files plus rows
- [ ] Message and notification `IsRead` flags update in the database
- [ ] Guest can message a host from a property page; host can reply from booking/check-in screens
- [ ] New messages update the recipient's badge (and open conversation) live via SignalR
- [ ] Only the SuperAdmin can promote a Guest to Admin
- [ ] ActivityLogs cover login, role change, and booking status update
- [ ] The database can be dropped and rebuilt from migrations alone
