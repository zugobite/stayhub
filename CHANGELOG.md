# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

- Profile management: any signed-in user can edit their name, location and introduction, and upload or remove a profile photo. Uploads surface on the listing host card, in testimonials, and in the header, replacing the generated portrait
- `ActivityType.ProfileUpdated`, so profile changes join the audit trail
- Pagination on all eight data tables at 15 rows per page, via a reusable `PagedList<T>` and `_Pagination` partial. Page links carry the existing query string forward, so filters, search terms and sort order survive paging. The activity log pages in SQL rather than in memory, since it grows without bound
- Per-person pricing: `PricePerNight` is now charged per guest per night, with `CalculateSubtotal/TotalPrice/ServiceFee` taking a guest count and the live breakdown reacting to the guest field
- Simulated card gateway (`StayPay Sandbox`) behind `IPaymentGateway`: a checkout page pre-filled with a sandbox card, Luhn and expiry validation, approving and declining test cards, a reference and last-4 on the receipt, and declines recorded as `PaymentStatus.Failed`
- `Province` enum and `HostProperty.Province`, with an index, so listings can be browsed by region
- `ApplicationUser.Bio` and `ApplicationUser.Location`, surfaced as a host profile card on the listing page
- Advanced gallery search: keyword, province, city, stay type, price range, guests, bedrooms, bathrooms, required amenities (AND-matched), and five sort orders
- `IAmenityRepository` backing the amenity filter checkboxes
- Seed data expanded from 2 listings to 15 across all nine provinces, with five hosts, nine reviews, and six new listing illustrations

### Changed

- `ProfilePictureUrl` is now actually used; previously the column existed but nothing read or wrote it
- Manual test script corrected (it still quoted the pre-rescale R 3 100 stay) and extended with scenarios for the payment gateway, advanced search, pagination and profiles; README gained the Payments and Profile routes
- Seed nightly rates rescaled to per-person figures so totals stay realistic (a R 8 900 villa is now R 1 150 per person)
- "Pay Now" opens the gateway checkout instead of charging inline
- Seeded host tenure is now fixed rather than random, so a rebuilt database reproduces the same "hosting since" dates

### Removed

- `BookingsController.Pay`, an orphaned endpoint that charged a booking inline and bypassed the gateway entirely. No view referenced it, but it remained a reachable POST

### Fixed

- The live price breakdown rendered its `data-*` values in the server's culture. Under a comma-decimal locale `parseFloat("0,1200")` returned `0`, so the service fee displayed as R 0 and the quoted total fell 12% short of the amount actually charged. These attributes are now written with invariant culture.

---

## [0.1.0] - 2026-08-09

Initial scaffold of the StayHub solution, mirroring the SkyFlow repository layout with an ASP.NET Core MVC web interface in place of the terminal UI.

### Added

- **Solution structure:**
  - Three-project layered solution: `StayHub.Core`, `StayHub.Data`, `StayHub.Web`
  - `docker-compose.yml` running SQL Server 2022 pinned to `linux/amd64` for Apple Silicon
  - `.github` issue, pull request, security, ownership, and funding templates
  - `sql/` scripts for database creation, teardown, and rubric verification queries

- **Domain layer (`StayHub.Core`):**
  - 16 domain models including `ApplicationUser`, `HostProperty`, `Booking`, `HostApplication`, `CheckInProcess`, `GuestCheckIn`, and `GuestDocument`
  - 8 enums covering booking, payment, application, activity, notification, check-in, document, and property-type states
  - `Roles` constants so `[Authorize(Roles = ...)]` cannot be mistyped silently
  - 10 repository interfaces defining the data access contracts

- **Data layer (`StayHub.Data`):**
  - `StayHubDbContext` extending `IdentityDbContext<ApplicationUser>` with 15 domain tables
  - Fluent configurations per aggregate, with restricted delete behaviour where cascade paths would converge on `AspNetUsers`
  - `InitialCreate` EF Core migration covering the Identity schema and every domain table
  - `RoleSeeder` creating the four roles and the SuperAdmin account on startup
  - `DataSeeder` inserting amenities plus demonstration accounts, listings, and a booking
  - `StayHubDbContextFactory` so `dotnet ef` works without booting the web host

- **Web layer (`StayHub.Web`):**
  - 11 MVC controllers covering the public site, the guest experience, and the Host and Admin dashboards
  - Registration assigning the Guest role automatically, with login, logout, and registration all writing activity log entries
  - Host application flow rejecting submissions without at least one photograph
  - Admin approval swapping the Host role in and the Guest role out atomically, and publishing the listing
  - Booking engine pricing stays as `(PricePerNight × Nights) + CleaningFee + ServiceFee`, with a live client-side breakdown
  - Multi-step guest check-in driven by the host's `StepsJson`, ending in a document upload the host verifies
  - Notification centre with ALL / UNREAD / READ / TODAY filters and persisted read state
  - Database-backed message inbox with conversation threads and persisted read state
  - SuperAdmin-only Guest-to-Admin promotion
  - Original "Cape Aegean" stylesheet layered over Bootstrap 5, in ZAR throughout

- **Documentation:**
  - `README.md` with setup, route reference, project structure, and class hierarchy
  - `docs/001-APPLICATION_REQUIREMENTS.md` - the assignment brief
  - `docs/002-MANUAL_TEST_SCRIPT.md` - a walkthrough covering every rubric row
  - `docs/003-ARCHITECTURE_AND_DESIGN.md` - architecture, ER diagram, and OOP pillar mapping

[Unreleased]: https://github.com/zugobite/stayhub/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/zugobite/stayhub/releases/tag/v0.1.0
