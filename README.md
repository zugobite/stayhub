# StayHub

A managed property rental ecosystem built with C# and ASP.NET Core MVC, demonstrating role-based identity, EF Core persistence, and a layered domain architecture behind an original "Cape Aegean" web interface.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Features

- **Role-Based Identity** - SuperAdmin, Admin, Host, and Guest roles enforced with `[Authorize(Roles = ...)]` on every dashboard route
- **Guest → Host Lifecycle** - Approving a host application adds the Host role and removes Guest atomically; a user is never both
- **Host Application Flow** - Property details plus at least one photograph, validated server-side before submission is accepted
- **Property Discovery** - Gallery of active listings across all nine provinces, with wishlist hearts and a photo carousel
- **Advanced Search** - Keyword, province, city, stay type, price range, guests, bedrooms, bathrooms, required amenities, and five sort orders, all composable in one query string
- **Host Profiles** - Every listing shows who you would be staying with: bio, home town, hosting-since date, listing and review counts, and their other stays
- **Booking Engine** - Date and traveller selection with a live price breakdown matching `(PricePerNight × Nights × Guests) + CleaningFee + ServiceFee`, priced per person per night
- **Simulated Payment Gateway** - "Pay Now" hands off to a sandbox checkout with pre-filled test cards, Luhn and expiry validation, approving and declining cards, and a referenced receipt. No money moves and only the last four digits are stored
- **Multi-Step Guest Check-in** - Host-defined steps stored as JSON, ending in an ID or passport upload the host verifies
- **Host Dashboard** - Listing management with an active toggle, booking approvals, earnings, reviews, and document verification
- **Admin Dashboard** - Application review queue, platform statistics, all bookings and listings, and SuperAdmin-only role promotion
- **Messaging & Notifications** - Database-backed inbox and a notification centre with ALL / UNREAD / READ / TODAY filters
- **Activity Logging** - Every login, role change, and booking status update is recorded for audit
- **EF Core Migrations** - The schema is code-first and rebuilds cleanly from scratch; roles and demo data seed on startup
- **Modular Architecture** - Multi-project solution with clean separation of Domain, Data, and UI layers

## Table of Contents

- [Quick Start](#quick-start)
- [Documentation](#documentation)
- [Routes & Actions](#routes--actions)
- [Usage Examples](#usage-examples)
- [Project Structure](#project-structure)
- [Class Hierarchy](#class-hierarchy)
- [Development](#development)
- [Contributing](#contributing)
- [Disclaimer](#disclaimer)
- [Security](#security)
- [License](#license)

## Quick Start

### Prerequisites

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** or higher
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (recommended for the database)
- *Alternatively:* a local SQL Server instance

### Cross-Platform Setup (Windows & macOS)

1. **Clone the repository:**
   ```bash
   git clone https://github.com/zugobite/stayhub.git
   cd stayhub
   ```

2. **Start the database:**
   ```bash
   docker compose up -d
   ```
   *Wait about 30 seconds for SQL Server to accept connections and the database to be created.*

3. **Run the application:**
   ```bash
   dotnet run --project src/StayHub.Web
   ```
   The first start applies the EF Core migrations and seeds the roles, the SuperAdmin, and demonstration data.

4. **Browse to** `https://localhost:7244`.

### OS-Specific Manual Setup

#### Windows (local SQL Server)

1. Ensure SQL Server (Express or Developer) is running.
2. Create the database:
   ```powershell
   sqlcmd -S localhost -i sql/001-create-database.sql
   ```
3. Update the `StayHubDB` connection string in `src/StayHub.Web/appsettings.json`.
4. Run the app - migrations apply automatically.

#### macOS

SQL Server does not run natively on macOS. Use the Docker setup above, or point the connection string at a remote instance.

### Default Login Credentials

| Role           | Email                      | Password         | Permissions                                          |
| :------------- | :------------------------- | :--------------- | :--------------------------------------------------- |
| **SuperAdmin** | `superadmin@stayhub.co.za` | `SuperAdmin123!` | Everything an Admin can do, plus promoting to Admin  |
| **Admin**      | `admin@stayhub.co.za`      | `Password123!`   | Review applications, view users, bookings, audit log |
| **Host**       | `host@stayhub.co.za`       | `Password123!`   | Manage listings, approve bookings, verify documents  |
| **Guest**      | `guest@stayhub.co.za`      | `Password123!`   | Browse, book, wishlist, complete check-in            |

> Change the SuperAdmin credentials via the `SeedUsers:SuperAdmin` section of `appsettings.json` before any real deployment.

---

## Documentation

Developer documentation lives in the [`docs/`](docs/) folder:

| Document                                                                 | Description                                     |
| ------------------------------------------------------------------------ | ----------------------------------------------- |
| [001-APPLICATION_REQUIREMENTS.md](docs/001-APPLICATION_REQUIREMENTS.md)   | Full assignment brief and requirements analysis |
| [002-MANUAL_TEST_SCRIPT.md](docs/002-MANUAL_TEST_SCRIPT.md)               | Step-by-step walkthrough covering every rubric row |
| [003-ARCHITECTURE_AND_DESIGN.md](docs/003-ARCHITECTURE_AND_DESIGN.md)     | Architecture, ER diagram, and OOP pillar mapping |

## Routes & Actions

### Public

| Route                    | Description                                          |
| ------------------------ | ---------------------------------------------------- |
| `/`                      | Landing page with featured listings                  |
| `/Properties`            | Gallery of active listings, filtered by city or type |
| `/Properties/View/{id}`  | Listing detail, amenities, reviews, booking widget   |
| `/Account/Login`         | Sign in                                              |
| `/Account/Register`      | Register (assigns the Guest role)                    |

### Guest

| Route                          | Description                                  |
| ------------------------------ | -------------------------------------------- |
| `/Bookings`                    | My trips, with cancel and check-in entry points |
| `/CheckIn/Start/{bookingId}`   | Multi-step check-in wizard and document upload |
| `/Wishlist`                    | Saved listings                                |
| `/HostApplications/Apply`      | Become a Host                                 |
| `/HostApplications/Status`     | Application review status                     |

### Host

| Route                        | Description                                    |
| ---------------------------- | ---------------------------------------------- |
| `/Host/Dashboard`            | Stat cards, performance metrics, listing cards |
| `/Host/Properties`           | Listing management and the active toggle       |
| `/Host/BookingRequests`      | Approve or reject pending bookings             |
| `/Host/CheckInProcess/{id}`  | Define the arrival steps for a listing         |
| `/Host/Verifications`        | Verify guest identity documents                |
| `/Host/Earnings`             | Earnings report                                |
| `/Host/Reviews`              | Reviews across all listings                    |

### Admin

| Route                             | Description                                        |
| --------------------------------- | -------------------------------------------------- |
| `/Admin/Dashboard`                | Platform statistics and recent activity tables     |
| `/Admin/Users`                    | User table; promotion controls for the SuperAdmin  |
| `/Admin/HostRequests`             | Application review queue                           |
| `/Admin/ReviewApplication/{id}`   | Approve or reject one application                  |
| `/Admin/Bookings`                 | Every booking on the platform                      |
| `/Admin/ActivityLogs`             | System audit trail, filterable by activity type    |

### Shared

| Route                                | Description                                            |
| ------------------------------------ | ------------------------------------------------------ |
| `/Notifications?filter=&selectedId=` | Notification centre with ALL / UNREAD / READ / TODAY   |
| `/Messages`                          | Inbox and sent folders                                 |
| `/Messages/Conversation/{userId}`    | Threaded conversation with a reply box                 |

## Usage Examples

### Complete user flow

```bash
# 1. Start the database and the app
docker compose up -d
dotnet run --project src/StayHub.Web

# 2. Register at /Account/Register  → you are a Guest
# 3. Apply at /HostApplications/Apply → attach at least one photo
# 4. Sign in as admin@stayhub.co.za → /Admin/HostRequests → Approve
#    (Host role added, Guest role removed, listing published)
# 5. Sign in as guest@stayhub.co.za → /Properties → pick dates → Reserve
# 6. Sign in as the Host → /Host/BookingRequests → Approve
# 7. As the Guest → /Bookings → Start Check-in → upload ID
# 8. As the Host → /Host/Verifications → Verify → booking becomes CheckedIn
```

### Quick start commands

```bash
# Build the solution
dotnet build

# Run the application
dotnet run --project src/StayHub.Web
```

## Project Structure

```
stayhub/
├── .github/
│   ├── ISSUE_TEMPLATE/                       # Issue templates
│   │   ├── bug_report.md
│   │   └── feature_request.md
│   ├── PULL_REQUEST_TEMPLATE.md              # PR template
│   ├── SECURITY.md                           # Security policy
│   ├── CODEOWNERS                            # Code ownership rules
│   └── FUNDING.yml                           # Sponsorship info
├── src/
│   ├── StayHub.Core/                         # Domain layer (no DB or UI dependency)
│   │   ├── Constants/
│   │   │   └── Roles.cs                      # SuperAdmin / Admin / Host / Guest
│   │   ├── Enums/
│   │   │   ├── BookingStatus.cs              # Pending → Approved → Paid → CheckedIn …
│   │   │   ├── PaymentStatus.cs              # Pending / Paid / Failed / Refunded
│   │   │   ├── ApplicationStatus.cs          # Pending / Approved / Rejected
│   │   │   ├── ActivityType.cs               # Login / RoleChange / BookingStatusUpdate …
│   │   │   ├── NotificationType.cs           # BookingApproved / NewMessage / …
│   │   │   ├── CheckInStatus.cs              # NotStarted → PendingVerification → Verified
│   │   │   ├── DocumentStatus.cs             # Pending / Verified / Rejected
│   │   │   └── PropertyType.cs               # Apartment / House / Room / Villa / …
│   │   ├── Models/
│   │   │   ├── ApplicationUser.cs            # Extends IdentityUser
│   │   │   ├── HostProperty.cs               # Listing; owns pricing and activation
│   │   │   ├── PropertyImage.cs              # Listing photograph
│   │   │   ├── Amenity.cs                    # Amenity lookup
│   │   │   ├── PropertyAmenity.cs            # Many-to-many join
│   │   │   ├── Booking.cs                    # Booking with status transitions
│   │   │   ├── Payment.cs                    # One-to-one with Booking
│   │   │   ├── Review.cs                     # Guest rating and comment
│   │   │   ├── WishlistItem.cs               # Saved listing
│   │   │   ├── Message.cs                    # Inbox message with IsRead
│   │   │   ├── Notification.cs               # Notification with IsRead
│   │   │   ├── ActivityLog.cs                # Audit trail entry
│   │   │   ├── HostApplication.cs            # Host / additional-property application
│   │   │   ├── CheckInProcess.cs             # Host-defined steps as JSON
│   │   │   ├── GuestCheckIn.cs               # One guest's run through the process
│   │   │   └── GuestDocument.cs              # Uploaded ID or passport
│   │   └── Interfaces/
│   │       ├── IPropertyRepository.cs        # Listing data contract
│   │       ├── IBookingRepository.cs         # Booking data contract
│   │       ├── IHostApplicationRepository.cs # Application data contract
│   │       ├── IMessageRepository.cs         # Inbox data contract
│   │       ├── INotificationRepository.cs    # Notification data contract
│   │       ├── IActivityLogRepository.cs     # Audit trail data contract
│   │       ├── IReviewRepository.cs          # Review data contract
│   │       ├── ICheckInRepository.cs         # Check-in data contract
│   │       ├── IWishlistRepository.cs        # Wishlist data contract
│   │       └── IUserRepository.cs            # User read-side data contract
│   ├── StayHub.Data/                         # Data access layer (EF Core)
│   │   ├── StayHubDbContext.cs               # IdentityDbContext + 15 domain tables
│   │   ├── StayHubDbContextFactory.cs        # Design-time factory for `dotnet ef`
│   │   ├── Configurations/                   # One IEntityTypeConfiguration per aggregate
│   │   ├── Migrations/                       # EF Core migrations (schema source of truth)
│   │   ├── Repositories/                     # Interface implementations
│   │   └── Seeders/
│   │       ├── RoleSeeder.cs                 # Roles + SuperAdmin on startup
│   │       └── DataSeeder.cs                 # Amenities + demonstration data
│   └── StayHub.Web/                          # Presentation layer (ASP.NET Core MVC)
│       ├── Program.cs                        # DI, Identity, pipeline, migrate + seed
│       ├── appsettings.json                  # Connection string and seed credentials
│       ├── Controllers/
│       │   ├── HomeController.cs             # Landing page
│       │   ├── AccountController.cs          # Register / login / logout + activity log
│       │   ├── PropertiesController.cs       # Gallery and detail pages
│       │   ├── BookingsController.cs         # Guest booking engine
│       │   ├── HostApplicationsController.cs # Become a Host
│       │   ├── HostController.cs             # Host Dashboard
│       │   ├── AdminController.cs            # Admin Dashboard
│       │   ├── CheckInController.cs          # Guest check-in wizard
│       │   ├── MessagesController.cs         # Inbox
│       │   ├── NotificationsController.cs    # Notification centre
│       │   └── WishlistController.cs         # Saved listings
│       ├── Services/
│       │   ├── IFileStorageService.cs        # Upload contract
│       │   ├── FileStorageService.cs         # wwwroot/uploads implementation
│       │   ├── IRoleTransitionService.cs     # Role swap contract
│       │   └── RoleTransitionService.cs      # Guest→Host and Guest→Admin swaps
│       ├── ViewModels/                       # Form and dashboard view models
│       ├── Views/                            # Razor views per controller
│       │   └── Shared/                       # Layout, sidebars, card partials
│       └── wwwroot/
│           ├── css/stayhub.css               # "Cape Aegean" theme and design tokens
│           ├── js/site.js                    # Live price breakdown
│           ├── favicon.svg                   # Brand mark, tab icon
│           └── images/
│               ├── brand/stayhub-mark.svg    # Logo mark (arch and sun)
│               ├── placeholder.svg           # Listing fallback image
│               └── seed/                     # Demonstration listing imagery
├── sql/                                      # Database scripts
│   ├── 001-create-database.sql               # Create StayHubDB
│   ├── 002-drop-database.sql                 # Drop for a clean rebuild
│   └── 003-verification-queries.sql          # Prove rubric behaviours persisted
├── docs/                                     # Developer documentation
│   ├── 001-APPLICATION_REQUIREMENTS.md       # Assignment brief
│   ├── 002-MANUAL_TEST_SCRIPT.md             # Manual test walkthrough
│   └── 003-ARCHITECTURE_AND_DESIGN.md        # Architecture and OOP mapping
├── .gitignore                                # Git ignore rules
├── CHANGELOG.md                              # Version history
├── CONTRIBUTING.md                           # Contribution guidelines
├── LICENSE                                   # MIT License
├── README.md                                 # This file
├── docker-compose.yml                        # SQL Server container
└── StayHub.sln                               # .NET solution file
```

## Class Hierarchy

| Class                        | Type          | Description                                                |
| ---------------------------- | ------------- | ---------------------------------------------------------- |
| `ApplicationUser`            | Model         | Extends `IdentityUser` with profile fields                 |
| `HostProperty`               | Model         | Listing with encapsulated activation and pricing           |
| `Booking`                    | Model         | Booking with guarded status transitions                    |
| `Payment`                    | Model         | One-to-one settlement record for a booking                 |
| `Review`                     | Model         | Guest rating and comment on a listing                      |
| `Amenity` / `PropertyAmenity`| Model         | Amenity lookup and its many-to-many join                   |
| `PropertyImage`              | Model         | Listing photograph, one flagged as the cover               |
| `WishlistItem`               | Model         | Saved listing per user                                     |
| `Message`                    | Model         | Inbox message with a persisted `IsRead` flag               |
| `Notification`               | Model         | Notification with `IsRead` and a "Take Action" URL         |
| `ActivityLog`                | Model         | Audit entry with an `ActivityType`                         |
| `HostApplication`            | Model         | Initial or additional-property application                 |
| `CheckInProcess`             | Model         | Host-defined arrival steps serialised as JSON              |
| `GuestCheckIn`               | Model         | One guest's progress through a check-in process            |
| `GuestDocument`              | Model         | Uploaded identity document awaiting verification           |
| `Roles`                      | Constants     | Compile-time-safe role names used by `[Authorize]`         |
| `StayHubDbContext`           | Data          | `IdentityDbContext<ApplicationUser>` plus 15 domain tables |
| `RoleSeeder`                 | Data          | Seeds the four roles and the SuperAdmin on startup         |
| `DataSeeder`                 | Data          | Seeds amenities and demonstration data                     |
| `PropertyRepository` et al.  | Data          | EF Core implementations of the Core interfaces             |
| `RoleTransitionService`      | Service       | Guest→Host and Guest→Admin swaps, with audit logging       |
| `FileStorageService`         | Service       | Validates and stores uploads under `wwwroot/uploads`       |
| `AccountController`          | Controller    | Registration, sign-in, sign-out                            |
| `HostController`             | Controller    | Host Dashboard and property management                     |
| `AdminController`            | Controller    | Application review, users, audit log                       |
| `CheckInController`          | Controller    | Multi-step guest check-in                                  |
| `Program`                    | Entry         | DI wiring, migration, seeding, HTTP pipeline               |

## Development

```bash
# Build the solution
dotnet build

# Run the application
dotnet run --project src/StayHub.Web

# Add a migration after changing the domain model
dotnet ef migrations add <Name> --project src/StayHub.Data --startup-project src/StayHub.Data

# Rebuild the database from scratch
sqlcmd -S localhost,1433 -U sa -P 'Password123!' -i sql/002-drop-database.sql
dotnet run --project src/StayHub.Web
```

### Architecture

This project follows a layered architecture with clean project separation:

- **StayHub.Core** defines domain models, enums, role constants, and repository interfaces
- **StayHub.Data** implements those interfaces with EF Core against SQL Server, and owns the migrations
- **StayHub.Web** handles routing, authorisation, view rendering, and file uploads

### OOP & Technical Features

| Feature                | Implementation                                                              |
| ---------------------- | --------------------------------------------------------------------------- |
| Abstraction            | `IPropertyRepository` and friends separate the database from controllers    |
| Inheritance            | `ApplicationUser : IdentityUser`; `StayHubDbContext : IdentityDbContext<T>` |
| Polymorphism           | Repositories resolved by interface through DI; `IValidatableObject` hooks   |
| Encapsulation          | Private `_status` / `_isActive` fields with guarded transition methods      |
| EF Core                | Code-first migrations, fluent configuration, LINQ query composition         |
| ASP.NET Core Identity  | PBKDF2 password hashing, role management, cookie authentication            |
| Role-Based Security    | `[Authorize(Roles = ...)]` plus ownership re-checks in the data layer       |
| Anti-Forgery           | `[ValidateAntiForgeryToken]` on every state-changing POST                   |
| Activity Logging       | Login, role change, and booking status changes recorded with a timestamp    |

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feat/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feat/amazing-feature`)
5. Open a Pull Request

### Guidelines

- Follow the existing code style
- Add XML documentation comments for new classes and methods
- Update documentation as needed
- Keep commits atomic and well-described

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

## Disclaimer

This project is created **purely for educational and portfolio demonstration purposes** to showcase C# development skills and software engineering knowledge.

**Important notices:**

- It implements **industry-standard patterns** documented in publicly available resources including:
  - Object-oriented design principles (abstraction, inheritance, polymorphism, encapsulation)
  - Repository pattern for data access separation
  - Layered architecture with clean project boundaries
- It is **not affiliated with, endorsed by, or derived from Airbnb, Inc.** or any other rental platform. The StayHub name, logo mark, colour palette, and typography are original to this project; no third-party brand colours, logos, or proprietary typefaces are used
- Only the **functional requirements** of the assignment brief (host application flow, booking dashboards) are modelled on the rental-marketplace category generally
- No payment gateway is integrated; payment status is advanced manually, which the brief permits
- This is **not intended for production use** without proper security audits and additional hardening

## Security

If you discover a security vulnerability, please report it privately rather than opening a public issue. See [SECURITY.md](.github/SECURITY.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
