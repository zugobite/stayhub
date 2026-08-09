# Staybnb - Airbnb Clone (Programming 742, Assignment 1)

> **Purpose of this file:** complete, self-contained context for an LLM assisting with this assignment. Everything below is derived from the official assignment brief issued by Richfield Graduate Institute of Technology.

---

## 1. Assignment Metadata

| Field               | Value                                                              |
| ------------------- | ------------------------------------------------------------------ |
| Module              | Programming 742                                                    |
| Assignment          | Assignment 1                                                       |
| Project name        | **Staybnb**                                                        |
| Total marks         | 100                                                                |
| Institution         | Richfield Graduate Institute of Technology (Pty) Ltd               |
| Deliverable         | Single zipped folder (source code + PDF report + supporting files) |
| Max upload size     | 50 MB                                                              |
| Submission platform | Moodle                                                             |

### Cover page fields to complete

- Name and Surname
- Student ITS No.
- Qualification
- Year of Study
- Semester
- Assignment due date
- Date submitted
- Student signature + date (signed declaration of originality)

### Document formatting rules (for the written report)

- Times New Roman, size 12, **1.5 line spacing** throughout.
- **Harvard Referencing Style** for all citations and references.
- Essay-style sections required: Table of Contents → Introduction → Main Body (with relevant subheadings) → Conclusion → References.
- Submit as **PDF** on Moodle.
- Use the specified cover page provided.
- Include a signed declaration of originality.

### Declaration of originality (verbatim intent)

The student declares the assignment is their own work, not copied except where acknowledged, all sources properly cited, and the submission complies with institutional academic integrity and plagiarism policies.

---

## 2. Technology Stack (mandatory)

| Layer     | Technology                                     |
| --------- | ---------------------------------------------- |
| Framework | **ASP.NET Core MVC**                           |
| ORM       | **EF Core** (Entity Framework Core)            |
| Database  | **SQL Server**                                 |
| Auth      | ASP.NET Core Identity (roles-based)            |
| Real-time | **Not required** - SignalR explicitly excluded |

Observed in the brief's screenshots (useful conventions, not hard requirements):

- Local dev URL: `https://localhost:7244`
- Bootstrap-based UI with Airbnb-style pink/red accent (`#FF385C`-like)
- Currency: **ZAR (R)**, formatted e.g. `R 1 780,00` / `R 3 100` per night
- Route patterns: `/Host/Dashboard`, `/Properties`, `/Properties/View/{id}`, `/Admin/Dashboard`, `/Notifications?selectedId={id}`

---

## 3. Project Overview

Build **Staybnb**, a managed property rental ecosystem inspired by the core functionality of **Airbnb**. The goal is to replicate the seamless transition between being a traveler and a lodging provider.

The heart of the project is the **User Lifecycle**:

1. A user explores the platform as a **Guest**.
2. The user applies to become a **Host**.
3. **Administrators** maintain the quality and security of the marketplace.

For UI/UX and feature-flow inspiration, study the Airbnb website - specifically the hosting application flow and guest booking dashboards.

> ⚠️ **Naming inconsistency in the brief:** the product is called _Staybnb_ throughout, but one requirement line refers to "StayHub" ("In StayHub, a user is either a Guest or a Host, not both"). Treat these as the same system; use **Staybnb** consistently in the implementation.

---

## 4. Functional Requirements

### 4.1 Identity & Role Evolution

- **Initial State:** All new registrants are automatically assigned the **Guest** role.
- **SuperAdmin Seeding:** The `RoleSeeder` must create a **"SuperAdmin"** on startup. This user is the **only** one capable of promoting Guests to the **Admin** role.
- **The Transition:** When an Admin approves a `HostApplication`, the system must programmatically:
  1. Add the **Host** role to the user.
  2. **Remove the Guest role** - a user is either a Guest or a Host, never both.
- **Access Control:** Strict use of `[Authorize(Roles = "RoleName")]` to protect dashboard routes. A Host must not access Guest-only pages, and vice-versa.

**Role set:** `SuperAdmin`, `Admin`, `Host`, `Guest`.

### 4.2 The Hosting Workflow (Host Role)

- **The Application:** Inspired by Airbnb's _"Become a Host"_ flow. Guests apply by providing property details - **Title, Description, Price, Address** - **and at least one `PropertyImage`**. An application is **incomplete and cannot be submitted without an image** (enforce this in validation).
- **Host Dashboard:** Once approved, the UI shifts to a Host-centric view where the Host can:
  - Manage property listings and toggle `IsActive` status.
  - Review pending Booking requests (Approve / Reject).
  - Configure the `CheckInProcess` - defining the rules and required documents for their specific property.

**Host Dashboard layout observed in the brief:**

- Sidebar: Dashboard, My Properties, Booking Requests, Earnings, Reviews, Messages, Notifications (with unread badge), Logout.
- Welcome banner with quick actions: _Add New Property_, _View Booking Requests_, _View Earnings Report_.
- Stat cards: Total Properties (with active count), Total Bookings (this month), Total Earnings (this month), Average Rating (review count).
- A "Performance Metrics" section below the stat cards.
- Property cards show: image with photo count, title, location, guests / bedrooms / beds / bathrooms, price per night, property type badge, Active/Inactive badge, booking count, review count, and actions (View Details, Edit, Deactivate).

### 4.3 The Guest Experience (Guest Role)

- **Property Discovery:** Browse **active** listings in a clean, Airbnb-style gallery. Include **filtering by city or property type**.
- **Booking Engine:** Guests select dates and number of travelers. The system must calculate:

```
TotalPrice = (PricePerNight × Nights) + CleaningFee + ServiceFee
```

- **Guest Check-in:** For approved bookings, guests complete a **multi-step workflow**, including uploading a `GuestDocument` (ID or Passport) for the Host to verify.

**Guest-facing UI observed in the brief:**

- Top search bar with "Anywhere / Any week / Add guests" + search button (Airbnb-style pill).
- Header links: _Host Application Status_, _Become a Host_, hamburger + avatar menu.
- Home hero copy: "Relax. Unwind. Choose your next stay." with tabs _Properties_ / _Become a Host_.
- Listing cards: image carousel with wishlist heart, location, title, price per night, star rating with review count.
- Property detail page: title, location, guests / bedrooms / beds / baths / rating, photo gallery (1 large + 2 thumbnails), description, **Amenities** grid (WiFi, Kitchen, Washer, Dryer, Air Conditioning, Heating), and a sticky booking widget with price, CHECK IN / CHECKOUT date pickers, GUESTS stepper, and a price breakdown (`R 3 100 × 1 nights` → `R 3 100`, `Service fee` → `R 372`).

### 4.4 Administrative Oversight (Admin Role)

Admin Dashboard = central command center:

- **Application Review:** Approve or reject new Host/Property applications.
- **User Management:** _(SuperAdmin only)_ Find existing Guests and promote them to the Admin role.
- **System Audit:** Monitor the `ActivityLog` to track security actions and significant system events.

**Admin Dashboard layout observed in the brief:**

- Sidebar: Dashboard, Users, Host Requests, Bookings, Properties, Messages, Notifications (badge), Activity Logs, Logout.
- Stat cards: Total Users (active users), Pending Host Apps (require review), Active Properties (listings available), Platform Revenue (total earnings), Total Bookings (pending count), Approved Hosts (active hosts).
- **Recent Activity** table: User (name + email), Action (e.g. `AdditionalPropertyApplicationSubmitted`, `HostApplicationSubmitted`), Date, Status.
- **Recent Host Applications** table: Applicant, Date, Status badge, Actions (view icon).
- **Recent Bookings** table: Guest, Dates, Status (e.g. Paid), Actions.

### 4.5 Communication & Logging

- **Messaging:** A **database-backed** messaging system. Not real-time (**no SignalR required**). Users must have an inbox to read and reply to queries.
- **Notifications:** A notification center (similar to the Airbnb bell icon) showing status updates like _"Booking Approved"_ or _"New Message."_
- **Activity Logging:** Use the `ActivityLog` model to track **every login, role change, and booking status update** for transparency.

**Notifications UI observed in the brief:**

- Header counters/filters: **ALL**, **UNREAD**, **READ**, **TODAY** with counts.
- "Mark all read" action; per-group "Mark as read".
- Grouped by day ("TODAY (2)").
- Master/detail: list on the left (icon, title, snippet, relative time, type tag), full detail on the right with READ/UNREAD state, a linked entity card (e.g. the property), and **Take Action** / **Delete** buttons.
- Example notifications: _"Host Application Approved"_ - "Congratulations! Your host application for 'Lovely City Apartment' has been approved…"; _"Additional Property Application Submitted"_ - "Your additional property application has been received and is under review."

---

## 5. Data Model

Use the entity relationships below to construct the model structure.

### 5.1 Entities

#### ApplicationUser

| Field             | Type        |
| ----------------- | ----------- |
| Id                | string (PK) |
| FirstName         | string      |
| LastName          | string      |
| ProfilePictureUrl | string      |
| CreatedAt         | DateTime    |

> Extends `IdentityUser`; `Id` is a string consistent with ASP.NET Core Identity.

#### HostProperty

| Field         | Type                          |
| ------------- | ----------------------------- |
| Id            | int (PK)                      |
| Title         | string(100)                   |
| Description   | string                        |
| PricePerNight | decimal(18,2)                 |
| HostId        | string (FK → ApplicationUser) |
| Address       | string                        |

> The brief also requires an **`IsActive`** toggle on properties (§4.2) and the UI implies City, PropertyType, MaxGuests, Bedrooms, Beds, Bathrooms, CleaningFee and ServiceFee - add these fields to satisfy filtering (§4.3) and the pricing formula.

#### Booking

| Field        | Type                          |
| ------------ | ----------------------------- |
| Id           | int (PK)                      |
| PropertyId   | int (FK → HostProperty)       |
| GuestId      | string (FK → ApplicationUser) |
| CheckInDate  | DateTime                      |
| CheckOutDate | DateTime                      |
| TotalPrice   | decimal(18,2)                 |
| Status       | `BookingStatus`               |

#### Payment

| Field     | Type               |
| --------- | ------------------ |
| Id        | int (PK)           |
| BookingId | int (FK → Booking) |
| Amount    | decimal(18,2)      |
| Status    | `PaymentStatus`    |
| CreatedAt | DateTime           |

#### Review

| Field      | Type                          |
| ---------- | ----------------------------- |
| Id         | int (PK)                      |
| PropertyId | int (FK → HostProperty)       |
| ReviewerId | string (FK → ApplicationUser) |
| Rating     | int (1–5)                     |
| Comment    | string(1000)                  |
| CreatedAt  | DateTime                      |

#### Amenity

| Field     | Type       |
| --------- | ---------- |
| Id        | int (PK)   |
| Name      | string(50) |
| IconClass | string(50) |

#### PropertyImage

| Field      | Type                    |
| ---------- | ----------------------- |
| Id         | int (PK)                |
| PropertyId | int (FK → HostProperty) |
| ImageUrl   | string                  |

#### WishlistItem

| Field      | Type                          |
| ---------- | ----------------------------- |
| Id         | int (PK)                      |
| UserId     | string (FK → ApplicationUser) |
| PropertyId | int (FK → HostProperty)       |
| CreatedAt  | DateTime                      |

#### Message

| Field      | Type                          |
| ---------- | ----------------------------- |
| Id         | int (PK)                      |
| SenderId   | string (FK → ApplicationUser) |
| ReceiverId | string (FK → ApplicationUser) |
| Content    | string                        |
| Timestamp  | DateTime                      |

> Add an `IsRead` flag - the rubric requires Read/Unread statuses to update correctly in the database.

#### ActivityLog

| Field        | Type                          |
| ------------ | ----------------------------- |
| Id           | int (PK)                      |
| UserId       | string (FK → ApplicationUser) |
| Action       | string(100)                   |
| ActivityType | `ActivityType`                |
| CreatedAt    | DateTime                      |

#### Notification

| Field     | Type                          |
| --------- | ----------------------------- |
| Id        | int (PK)                      |
| UserId    | string (FK → ApplicationUser) |
| Title     | string(100)                   |
| Type      | `NotificationType`            |
| CreatedAt | DateTime                      |

> Add an `IsRead` flag to support the ALL / UNREAD / READ / TODAY filters shown in the brief.

#### HostApplication

| Field             | Type                          |
| ----------------- | ----------------------------- |
| Id                | int (PK)                      |
| ApplicationUserId | string (FK → ApplicationUser) |
| PropertyId        | int (FK → HostProperty)       |
| Status            | `ApplicationStatus`           |
| AppliedAt         | DateTime                      |

#### CheckInProcess

| Field      | Type                    |
| ---------- | ----------------------- |
| Id         | int (PK)                |
| PropertyId | int (FK → HostProperty) |
| Title      | string(200)             |
| StepsJson  | string                  |
| CreatedAt  | DateTime                |

> `StepsJson` stores the multi-step check-in definition as serialised JSON.

#### GuestCheckIn

| Field            | Type                      |
| ---------------- | ------------------------- |
| Id               | int (PK)                  |
| BookingId        | int (FK → Booking)        |
| CheckInProcessId | int (FK → CheckInProcess) |
| Status           | `CheckInStatus`           |
| CreatedAt        | DateTime                  |

#### GuestDocument

| Field          | Type                    |
| -------------- | ----------------------- |
| Id             | int (PK)                |
| GuestCheckInId | int (FK → GuestCheckIn) |
| DocumentType   | string(100)             |
| FileName       | string(200)             |
| Status         | `DocumentStatus`        |

### 5.2 Key Relationships

| Cardinality  | Relationship                   | Note                                                                   |
| ------------ | ------------------------------ | ---------------------------------------------------------------------- |
| One-to-Many  | ApplicationUser → HostProperty | User can host multiple properties                                      |
| One-to-Many  | ApplicationUser → Booking      | User can have multiple bookings                                        |
| One-to-Many  | ApplicationUser → Review       | User can write multiple reviews                                        |
| One-to-Many  | HostProperty → Booking         | Property can have multiple bookings                                    |
| One-to-Many  | HostProperty → Review          | Property can have multiple reviews                                     |
| One-to-One   | Booking → Payment              | Each booking has one payment                                           |
| Many-to-Many | HostProperty ↔ Amenity         | Property can have many amenities; an amenity can be in many properties |
| One-to-Many  | HostProperty → PropertyImage   | Property can have multiple images                                      |
| One-to-One   | HostProperty → CheckInProcess  | Each property has one check-in process                                 |
| One-to-One   | Booking → GuestCheckIn         | Each booking has one guest check-in                                    |
| One-to-Many  | GuestCheckIn → GuestDocument   | Check-in can require multiple documents                                |

### 5.3 Enumerations referenced by the brief

These are named in the entity diagram; define them explicitly. Suggested members are inferred from the workflow described.

| Enum                | Used by                  | Suggested members                                                                                                        |
| ------------------- | ------------------------ | ------------------------------------------------------------------------------------------------------------------------ |
| `BookingStatus`     | Booking.Status           | `Pending`, `Approved`, `Rejected`, `Paid`, `CheckedIn`, `Completed`, `Cancelled`                                         |
| `PaymentStatus`     | Payment.Status           | `Pending`, `Paid`, `Failed`, `Refunded`                                                                                  |
| `ApplicationStatus` | HostApplication.Status   | `Pending`, `Approved`, `Rejected`                                                                                        |
| `ActivityType`      | ActivityLog.ActivityType | `Login`, `Logout`, `RoleChange`, `BookingStatusUpdate`, `ApplicationSubmitted`, `ApplicationReviewed`, `PropertyUpdated` |
| `NotificationType`  | Notification.Type        | `BookingApproved`, `BookingRejected`, `NewMessage`, `HostApplicationApproved`, `AdditionalPropertyApplication`, `System` |
| `CheckInStatus`     | GuestCheckIn.Status      | `NotStarted`, `InProgress`, `PendingVerification`, `Verified`, `Rejected`                                                |
| `DocumentStatus`    | GuestDocument.Status     | `Pending`, `Verified`, `Rejected`                                                                                        |

Observed status strings in the brief's screenshots: booking status **Paid**; application status **Appro…** (Approved); activity status **Active**; activity actions **`HostApplicationSubmitted`** and **`AdditionalPropertyApplicationSubmitted`**.

---

## 6. Core Workflows (state machines)

### 6.1 Guest → Host

```
Register  →  Guest role assigned automatically
   ↓
Submit HostApplication (Title, Description, Price, Address, ≥1 PropertyImage)
   ↓  status = Pending
Admin reviews in Admin Dashboard
   ↓  approve
Add "Host" role  +  Remove "Guest" role   →  ActivityLog(RoleChange)
   ↓
Notification: "Host Application Approved"  →  UI switches to Host Dashboard
```

### 6.2 Booking → Check-in

```
Guest selects dates + travelers
   ↓  TotalPrice = (PricePerNight × Nights) + CleaningFee + ServiceFee
Booking created  →  status = Pending
   ↓
Host approves/rejects in Booking Requests   →  ActivityLog(BookingStatusUpdate)
   ↓  Approved
Guest completes multi-step GuestCheckIn (driven by the property's CheckInProcess.StepsJson)
   ↓
Guest uploads GuestDocument (ID or Passport)
   ↓
Host verifies document  →  status = CheckedIn
```

### 6.3 Admin promotion (SuperAdmin only)

```
SuperAdmin seeded by RoleSeeder on startup
   ↓
SuperAdmin → User Management → find Guest → promote to Admin
   ↓
ActivityLog(RoleChange)
```

---

## 7. Grading Rubric (Total: 100 Marks)

| Category                               | Criteria                                                                                                                               | Marks   |
| -------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| **User Authentication and Role Logic** | Successful user login/registration and transition from Guest to Host. Roles are correctly swapped (Guest removed) upon Admin approval. | 20      |
| **Admin Dashboard**                    | Admins can review applications, view system logs, and (for SuperAdmin) assign Admin roles.                                             | 15      |
| **Host Dashboard**                     | Hosts can effectively manage their properties, approve bookings, and verify guest check-in documents.                                  | 15      |
| **Application Process**                | Logic ensures a property and its images are submitted correctly during the Host application phase.                                     | 15      |
| **Booking & Check-In**                 | Functional workflow from booking request to status updates (Pending → Approved → CheckedIn).                                           | 15      |
| **Messaging & Notifications**          | Functional inbox and notification list. Statuses (Read/Unread) update correctly in the database.                                       | 10      |
| **Data & Architecture**                | EF Core Migrations are accurate. Activity logging is integrated into key actions. Code is clean and organized.                         | 10      |
| **TOTAL**                              |                                                                                                                                        | **100** |

---

## 8. Submission Instructions

- Submit a **single zipped folder** containing the complete project files.
- Total file size **must not exceed 50 MB**.
- Submissions that exceed the size limit or are incomplete **will not be accepted**.
- Verify the application/system is **fully functional** before uploading to Moodle.
- All required documents must be included: **PDF report, source code, and any supporting files**.
- Include any **SQL scripts** used to seed initial data, if seeding was not done programmatically.
- **NB: A non-functional application will result in a zero mark.**

> Practical tip for the 50 MB cap: delete `bin/`, `obj/`, `.vs/`, and `node_modules/` before zipping, and keep seeded property images small/compressed.

---

## 9. Implementation Checklist

**Identity & roles**

- [ ] `ApplicationUser : IdentityUser` with FirstName, LastName, ProfilePictureUrl, CreatedAt
- [ ] `RoleSeeder` creates SuperAdmin, Admin, Host, Guest roles + the SuperAdmin user on startup
- [ ] Guest role auto-assigned on registration
- [ ] Approval swaps Host in / Guest out atomically
- [ ] `[Authorize(Roles = "...")]` on every dashboard route; Host ↔ Guest page isolation verified

**Host application**

- [ ] Property fields + ≥1 image validated server-side before submission
- [ ] Application blocked (with a clear error) when no image is attached
- [ ] Admin approve/reject updates `HostApplication.Status`

**Host features**

- [ ] Property CRUD + `IsActive` toggle
- [ ] Booking request list with Approve/Reject
- [ ] `CheckInProcess` builder writing to `StepsJson`
- [ ] Guest document verification screen

**Guest features**

- [ ] Gallery of active listings with city / property-type filters
- [ ] Date + traveler selection with live price breakdown
- [ ] `TotalPrice = (PricePerNight × Nights) + CleaningFee + ServiceFee`
- [ ] Multi-step check-in with document upload

**Admin features**

- [ ] Application review queue
- [ ] SuperAdmin-only Guest → Admin promotion
- [ ] ActivityLog viewer

**Cross-cutting**

- [ ] Message inbox with reply + IsRead
- [ ] Notification center with IsRead and type filters
- [ ] ActivityLog written on login, role change, and booking status update
- [ ] EF Core migrations committed and applying cleanly from scratch
- [ ] Seed data (programmatic or SQL script included in the zip)

---

## 10. Notes and Ambiguities to Resolve with the Lecturer

1. **"StayHub" vs "Staybnb"** - the brief uses both names; assumed to be the same system.
2. **CleaningFee and ServiceFee** appear in the pricing formula but not in the entity diagram - they must be added to `HostProperty` (or a settings/config table). The screenshot shows a service fee of `R 372` on a `R 3 100` single-night stay (≈12%), suggesting a percentage-based service fee.
3. **`IsActive`** is required by the requirements text but absent from the `HostProperty` diagram - add it.
4. **`IsRead`** is required for both Messages and Notifications by the rubric but absent from the diagrams - add it.
5. The screenshots show **additional property applications** (a Host applying to list a further property) as a distinct flow from the initial host application - consider modelling both through `HostApplication`.
6. `Payment` exists in the model and bookings display a **Paid** status, but no payment gateway is required by the brief - a simulated/manual payment status update is sufficient.
