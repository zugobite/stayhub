-- ============================================
-- StayHub
-- 003-verification-queries.sql
-- Read-only queries for demonstrating that the rubric's behaviours are
-- actually persisted, rather than only shown in the UI.
-- ============================================

USE StayHubDB;
GO

-- 1. Roles were seeded and the SuperAdmin exists.
SELECT r.Name AS RoleName, COUNT(ur.UserId) AS UserCount
FROM AspNetRoles r
LEFT JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
GROUP BY r.Name
ORDER BY r.Name;
GO

-- 2. The Guest role is swapped for Host on approval: no user holds both.
SELECT u.Email, STRING_AGG(r.Name, ', ') AS Roles
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r ON r.Id = ur.RoleId
GROUP BY u.Email
ORDER BY u.Email;
GO

-- 3. Host applications and the listings they proposed.
SELECT a.Id, u.Email AS Applicant, p.Title, a.Status, a.IsAdditionalProperty, a.AppliedAt, a.ReviewedAt
FROM HostApplications a
JOIN AspNetUsers u ON u.Id = a.ApplicationUserId
JOIN Properties p ON p.Id = a.PropertyId
ORDER BY a.AppliedAt DESC;
GO

-- 4. Every application has at least one image (the submission rule).
SELECT p.Id, p.Title, COUNT(i.Id) AS ImageCount
FROM Properties p
LEFT JOIN PropertyImages i ON i.PropertyId = p.Id
GROUP BY p.Id, p.Title
ORDER BY ImageCount ASC;
GO

-- 5. Booking lifecycle: Pending -> Approved -> CheckedIn.
SELECT b.Id, g.Email AS Guest, p.Title, b.CheckInDate, b.CheckOutDate, b.Status, b.TotalPrice
FROM Bookings b
JOIN AspNetUsers g ON g.Id = b.GuestId
JOIN Properties p ON p.Id = b.PropertyId
ORDER BY b.CreatedAt DESC;
GO

-- 6. Read/unread state is persisted for messages and notifications.
SELECT 'Messages' AS Source, IsRead, COUNT(*) AS Total FROM Messages GROUP BY IsRead
UNION ALL
SELECT 'Notifications', IsRead, COUNT(*) FROM Notifications GROUP BY IsRead;
GO

-- 7. Activity logging covers logins, role changes, and booking updates.
SELECT ActivityType, COUNT(*) AS Entries, MAX(CreatedAt) AS MostRecent
FROM ActivityLogs
GROUP BY ActivityType
ORDER BY Entries DESC;
GO
