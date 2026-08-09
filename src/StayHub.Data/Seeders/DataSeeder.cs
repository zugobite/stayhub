using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Core.Constants;
using StayHub.Core.Enums;
using StayHub.Core.Models;

namespace StayHub.Data.Seeders;

/// <summary>
/// Seeds the amenity lookup table plus a set of demonstration accounts, listings,
/// reviews, and bookings so every dashboard and search filter has something
/// meaningful to render on a freshly migrated database.
/// </summary>
/// <remarks>
/// The listings deliberately span all nine provinces and a wide price range so
/// the gallery's province, price, capacity, and amenity filters can each be
/// demonstrated without adding data by hand.
/// </remarks>
public static class DataSeeder
{
    private const string DemoPassword = "Password123!";

    /// <summary>
    /// Seeds reference and demonstration data. Demo account profiles are kept in
    /// sync on every startup; listings and related rows seed only when absent.
    /// </summary>
    /// <param name="services">The application service provider.</param>
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<StayHubDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedAmenitiesAsync(context);

        // Demo accounts are synced on every startup so name/bio changes (e.g. host
        // rename) apply without dropping the database. Listings still seed only once.
        var admin = await CreateUserAsync(userManager, "admin@stayhub.co.za", "Sarah", "Mitchell", Roles.Admin);
        var guest = await CreateUserAsync(userManager, "guest@stayhub.co.za", "David", "Botha", Roles.Guest,
            location: "Pretoria, Gauteng");
        var guest2 = await CreateUserAsync(userManager, "naledi@stayhub.co.za", "Naledi", "Khumalo", Roles.Guest,
            location: "Durban, KwaZulu-Natal");
        var guest3 = await CreateUserAsync(userManager, "johan@stayhub.co.za", "Johan", "Steyn", Roles.Guest,
            location: "Bloemfontein, Free State");
        var guest4 = await CreateUserAsync(userManager, "fatima@stayhub.co.za", "Fatima", "Adams", Roles.Guest,
            location: "Port Elizabeth, Eastern Cape");

        var zascia = await CreateUserAsync(userManager, "host@stayhub.co.za", "Zascia", "Hugo", Roles.Host,
            location: "Cape Town, Western Cape",
            bio: "Cape Town-based host who looks after a handful of stays around the Mother City. " +
                 "I am always happy to share local tips - quiet coffee spots, coastal walks, and the " +
                 "bakeries the tour buses miss.",
            monthsOnPlatform: 41);

        var pieter = await CreateUserAsync(userManager, "pieter@stayhub.co.za", "Pieter", "van der Merwe", Roles.Host,
            location: "Stellenbosch, Western Cape",
            bio: "Fourth-generation farmer in the Cape Winelands. Our cottage sits on the edge of the vineyard, " +
                 "and I am always happy to arrange a tasting or point you to a quieter farm stall.",
            monthsOnPlatform: 33);

        var ayesha = await CreateUserAsync(userManager, "ayesha@stayhub.co.za", "Ayesha", "Patel", Roles.Host,
            location: "Umhlanga, KwaZulu-Natal",
            bio: "I look after two places along the KZN coast. Expect clean linen, strong coffee, and a very " +
                 "detailed list of where to eat curry within walking distance.",
            monthsOnPlatform: 26);

        var sipho = await CreateUserAsync(userManager, "sipho@stayhub.co.za", "Sipho", "Dlamini", Roles.Host,
            location: "Hazyview, Mpumalanga",
            bio: "Former field guide turned host. My lodges sit close to the Kruger gates, and I will happily " +
                 "talk you through the best time of day to head out for game viewing.",
            monthsOnPlatform: 19);

        var lerato = await CreateUserAsync(userManager, "lerato@stayhub.co.za", "Lerato", "Mokoena", Roles.Host,
            location: "Johannesburg, Gauteng",
            bio: "Architect by trade, host by weekend. I restore older buildings in Joburg and the Free State " +
                 "and rent them out once the dust settles.",
            monthsOnPlatform: 11);

        await SyncLegacyHostNameInReviewsAsync(context);

        if (await context.Properties.AnyAsync())
            return;

        if (admin is null || guest is null || guest2 is null || guest3 is null || guest4 is null
            || zascia is null || pieter is null || ayesha is null || sipho is null || lerato is null)
        {
            return;
        }

        var amenities = await context.Amenities.ToListAsync();
        int Amenity(string name) => amenities.First(a => a.Name == name).Id;

        var listings = new List<(HostProperty Property, string[] Images, string[] Amenities)>
        {
            (Listing("Lovely City Apartment",
                "A bright, modern apartment a short walk from the V&A Waterfront. Sleeps four, with fast WiFi " +
                "and a fully equipped kitchen.",
                3100m, 350m, "42 Long Street", "Cape Town", Province.WesternCape, PropertyType.Apartment,
                4, 2, 3, 1, zascia.Id, 30),
                new[] { "/images/seed/apartment-1.svg", "/images/seed/apartment-2.svg" },
                new[] { "WiFi", "Kitchen", "Washer", "Dryer" }),

            (Listing("Garden Cottage Retreat",
                "A quiet cottage set in an established garden in Johannesburg's leafy north. Ideal for a " +
                "weekend away from the city noise.",
                1780m, 250m, "8 Jan Smuts Avenue", "Johannesburg", Province.Gauteng, PropertyType.Cottage,
                2, 1, 1, 1, lerato.Id, 14),
                new[] { "/images/seed/cottage-1.svg" },
                new[] { "WiFi", "Kitchen" }),

            (Listing("Camps Bay Villa with Pool",
                "Glass-fronted villa above Camps Bay beach. Heated pool, eight beds, and the whole Atlantic " +
                "seaboard laid out below the deck.",
                8900m, 900m, "17 Theresa Avenue", "Cape Town", Province.WesternCape, PropertyType.Villa,
                8, 4, 5, 3, zascia.Id, 26),
                new[] { "/images/seed/villa-pool.svg", "/images/seed/apartment-2.svg" },
                new[] { "WiFi", "Kitchen", "Pool", "Air Conditioning", "Free Parking", "Washer" }),

            (Listing("Umhlanga Beach House",
                "Family beach house a hundred metres from the promenade. Wide veranda, outdoor shower, and " +
                "the lighthouse visible from the front room.",
                4600m, 500m, "3 Lagoon Drive", "Umhlanga", Province.KwaZuluNatal, PropertyType.House,
                6, 3, 4, 2, ayesha.Id, 24),
                new[] { "/images/seed/beach-house.svg" },
                new[] { "WiFi", "Kitchen", "Air Conditioning", "Free Parking" }),

            (Listing("Kruger Gate Bushveld Lodge",
                "Thatched lodge fifteen minutes from the Phabeni gate. Outdoor boma, resident bushbabies, and " +
                "no traffic noise whatsoever.",
                5400m, 600m, "Plot 22, Sabie River Road", "Hazyview", Province.Mpumalanga, PropertyType.GuestHouse,
                6, 3, 3, 2, sipho.Id, 22),
                new[] { "/images/seed/bushveld-lodge.svg" },
                new[] { "WiFi", "Kitchen", "Pool", "Free Parking" }),

            (Listing("Vineyard Cottage, Stellenbosch",
                "Restored labourer's cottage on a working wine farm. Thick walls, a wood stove, and vines " +
                "running right up to the stoep.",
                2950m, 300m, "Bosmanshoek Farm, R44", "Stellenbosch", Province.WesternCape, PropertyType.Cottage,
                4, 2, 2, 1, pieter.Id, 20),
                new[] { "/images/seed/winelands.svg" },
                new[] { "WiFi", "Kitchen", "Heating", "Free Parking" }),

            (Listing("Karoo Farmstay",
                "Nineteenth-century farmhouse outside Graaff-Reinet. Enormous skies, complete silence, and a " +
                "stoep built for watching thunderstorms roll in.",
                1450m, 200m, "Rietfontein Farm, N9", "Graaff-Reinet", Province.EasternCape, PropertyType.House,
                4, 2, 3, 1, pieter.Id, 18),
                new[] { "/images/seed/karoo-cottage.svg" },
                new[] { "Kitchen", "Heating", "Free Parking" }),

            (Listing("Maboneng Warehouse Loft",
                "Double-volume loft in a converted warehouse. Steel windows, a proper coffee machine, and the " +
                "Sunday market downstairs.",
                2200m, 280m, "264 Fox Street", "Johannesburg", Province.Gauteng, PropertyType.Apartment,
                2, 1, 1, 1, lerato.Id, 16),
                new[] { "/images/seed/city-loft.svg" },
                new[] { "WiFi", "Kitchen", "Washer", "Air Conditioning" }),

            (Listing("Durban Beachfront Apartment",
                "Tenth-floor flat on the Golden Mile with uninterrupted sea views and secure parking downstairs.",
                2650m, 300m, "155 OR Tambo Parade", "Durban", Province.KwaZuluNatal, PropertyType.Apartment,
                4, 2, 2, 2, ayesha.Id, 15),
                new[] { "/images/seed/apartment-2.svg" },
                new[] { "WiFi", "Kitchen", "Air Conditioning", "Pool", "Free Parking" }),

            (Listing("Kalahari Stargazing Cabin",
                "Off-grid cabin outside Upington with a bed that rolls onto the deck. Some of the darkest skies " +
                "left in the country.",
                1980m, 220m, "Farm 41, Kalahari Road", "Upington", Province.NorthernCape, PropertyType.Cottage,
                2, 1, 1, 1, sipho.Id, 12),
                new[] { "/images/seed/karoo-cottage.svg" },
                new[] { "Kitchen", "Heating", "Free Parking" }),

            (Listing("Clarens Mountain Cottage",
                "Sandstone cottage looking onto the Maluti mountains. Ten minutes' walk from the village square " +
                "and its breweries.",
                1690m, 240m, "12 Van Reenen Street", "Clarens", Province.FreeState, PropertyType.Cottage,
                4, 2, 2, 1, lerato.Id, 11),
                new[] { "/images/seed/winelands.svg" },
                new[] { "WiFi", "Kitchen", "Heating", "Free Parking" }),

            (Listing("Hoedspruit Safari Suite",
                "Private suite on a wildlife estate. Giraffe wander past the plunge pool, and the airstrip is " +
                "twenty minutes away.",
                6200m, 700m, "Raptor's View Wildlife Estate", "Hoedspruit", Province.Limpopo, PropertyType.GuestHouse,
                4, 2, 2, 2, sipho.Id, 9),
                new[] { "/images/seed/bushveld-lodge.svg" },
                new[] { "WiFi", "Pool", "Air Conditioning", "Free Parking", "Kitchen" }),

            (Listing("Pilanesberg Game Cottage",
                "Self-catering cottage inside a private reserve bordering Pilanesberg. Braai area, game drives " +
                "on request, and zebra on the lawn most mornings.",
                3400m, 400m, "Black Rhino Reserve", "Rustenburg", Province.NorthWest, PropertyType.Cottage,
                5, 2, 3, 2, sipho.Id, 8),
                new[] { "/images/seed/karoo-cottage.svg" },
                new[] { "Kitchen", "Pool", "Free Parking", "WiFi" }),

            (Listing("Knysna Lagoon House",
                "Timber house on the lagoon edge with a jetty and a kayak. The Heads are a short drive, the " +
                "oysters are closer.",
                3850m, 450m, "9 Belvidere Road", "Knysna", Province.WesternCape, PropertyType.House,
                6, 3, 4, 2, zascia.Id, 6),
                new[] { "/images/seed/beach-house.svg" },
                new[] { "WiFi", "Kitchen", "Heating", "Free Parking", "Washer" }),

            (Listing("Jeffreys Bay Surf Room",
                "Simple private room a two-minute walk from Supertubes. Board storage, outdoor shower, and a " +
                "shared kitchen.",
                890m, 120m, "6 Pepper Street", "Jeffreys Bay", Province.EasternCape, PropertyType.Room,
                2, 1, 1, 1, ayesha.Id, 4),
                new[] { "/images/seed/cottage-1.svg" },
                new[] { "WiFi", "Kitchen" })
        };

        context.Properties.AddRange(listings.Select(l => l.Property));
        await context.SaveChangesAsync();

        foreach (var (property, images, amenityNames) in listings)
        {
            for (var i = 0; i < images.Length; i++)
            {
                context.PropertyImages.Add(new PropertyImage
                {
                    PropertyId = property.Id,
                    ImageUrl = images[i],
                    IsCover = i == 0
                });
            }

            foreach (var name in amenityNames)
                context.PropertyAmenities.Add(new PropertyAmenity { PropertyId = property.Id, AmenityId = Amenity(name) });
        }

        await context.SaveChangesAsync();

        await SeedReviewsAsync(context, listings.Select(l => l.Property).ToList(),
            new[] { guest.Id, guest2.Id, guest3.Id, guest4.Id });

        var apartment = listings[0].Property;

        context.HostApplications.Add(new HostApplication
        {
            ApplicationUserId = zascia.Id,
            PropertyId = apartment.Id,
            Status = ApplicationStatus.Approved,
            AppliedAt = DateTime.UtcNow.AddDays(-31),
            ReviewedAt = DateTime.UtcNow.AddDays(-30)
        });

        context.Bookings.Add(new Booking
        {
            PropertyId = apartment.Id,
            GuestId = guest.Id,
            CheckInDate = DateTime.UtcNow.Date.AddDays(7),
            CheckOutDate = DateTime.UtcNow.Date.AddDays(8),
            NumberOfGuests = 2,
            TotalPrice = apartment.CalculateTotalPrice(1),
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });

        await context.SaveChangesAsync();

        context.Notifications.Add(new Notification
        {
            UserId = zascia.Id,
            Title = "Host Application Approved",
            Body = "Congratulations! Your host application for 'Lovely City Apartment' has been approved.",
            Type = NotificationType.HostApplicationApproved,
            ActionUrl = $"/Properties/View/{apartment.Id}",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        });

        context.ActivityLogs.Add(new ActivityLog
        {
            UserId = zascia.Id,
            Action = "HostApplicationSubmitted",
            ActivityType = ActivityType.ApplicationSubmitted,
            Details = "Lovely City Apartment",
            CreatedAt = DateTime.UtcNow.AddDays(-31)
        });

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Builds an activated listing. Keeps the seed table above readable by pulling
    /// the repeated property initialisation into one place.
    /// </summary>
    private static HostProperty Listing(
        string title,
        string description,
        decimal pricePerNight,
        decimal cleaningFee,
        string address,
        string city,
        Province province,
        PropertyType type,
        int maxGuests,
        int bedrooms,
        int beds,
        int bathrooms,
        string hostId,
        int createdDaysAgo)
    {
        var property = new HostProperty
        {
            Title = title,
            Description = description,
            PricePerNight = pricePerNight,
            CleaningFee = cleaningFee,
            ServiceFeeRate = 0.12m,
            Address = address,
            City = city,
            Province = province,
            PropertyType = type,
            MaxGuests = maxGuests,
            Bedrooms = bedrooms,
            Beds = beds,
            Bathrooms = bathrooms,
            HostId = hostId,
            CreatedAt = DateTime.UtcNow.AddDays(-createdDaysAgo)
        };

        property.Activate();
        return property;
    }

    /// <summary>
    /// Adds guest reviews across a subset of listings so the gallery's "top rated"
    /// ordering has something to sort by. Listings are left deliberately unreviewed
    /// so the "no reviews yet" state remains reachable.
    /// </summary>
    private static async Task SeedReviewsAsync(
        StayHubDbContext context,
        IReadOnlyList<HostProperty> properties,
        IReadOnlyList<string> reviewerIds)
    {
        // (listing index, reviewer index, rating, comment, days ago). Reviewers are
        // spread deliberately so the landing page's testimonials come from
        // different guests rather than one very enthusiastic person.
        var reviews = new (int Index, int Reviewer, int Rating, string Comment, int DaysAgo)[]
        {
            (0, 0, 5, "Spotless, and exactly as described. The walk to the Waterfront takes ten minutes.", 12),
            (0, 1, 4, "Great flat, though the street gets a little noisy on a Friday.", 8),
            (2, 2, 5, "The view genuinely stops you in your tracks. Worth every rand.", 4),
            (2, 0, 5, "Pool was heated as promised in July, and every message was answered within minutes.", 9),
            (3, 3, 4, "Lovely house and a great location. Could do with a few more kitchen things.", 6),
            (4, 1, 5, "Woke up to hippo noise from the river. Sipho's game drive advice was spot on.", 3),
            (5, 3, 4, "Very peaceful, and the wood stove made it. Bring warm clothes in winter.", 7),
            (8, 2, 3, "Good views, but the lift was out for a day and it is a long way up.", 11),
            (11, 0, 5, "Giraffe at breakfast. Genuinely unforgettable, and immaculately kept.", 5),
            (13, 3, 5, "Kayak on the lagoon at sunrise was the highlight of our year.", 2),
            (6, 2, 4, "Silence like I have never known. The stoep is worth the drive on its own.", 10)
        };

        foreach (var (index, reviewer, rating, comment, daysAgo) in reviews)
        {
            if (index >= properties.Count || reviewer >= reviewerIds.Count)
                continue;

            context.Reviews.Add(new Review
            {
                PropertyId = properties[index].Id,
                ReviewerId = reviewerIds[reviewer],
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow.AddDays(-daysAgo)
            });
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Inserts the amenity lookup rows the property detail page renders.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    private static async Task SeedAmenitiesAsync(StayHubDbContext context)
    {
        if (await context.Amenities.AnyAsync())
            return;

        context.Amenities.AddRange(
            new Amenity { Name = "WiFi", IconClass = "wifi" },
            new Amenity { Name = "Kitchen", IconClass = "kitchen" },
            new Amenity { Name = "Washer", IconClass = "washer" },
            new Amenity { Name = "Dryer", IconClass = "dryer" },
            new Amenity { Name = "Air Conditioning", IconClass = "aircon" },
            new Amenity { Name = "Heating", IconClass = "heating" },
            new Amenity { Name = "Free Parking", IconClass = "parking" },
            new Amenity { Name = "Pool", IconClass = "pool" });

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Rewrites seed reviews that still name the previous demo host so existing
    /// databases pick up the rename without a full reseed.
    /// </summary>
    private static async Task SyncLegacyHostNameInReviewsAsync(StayHubDbContext context)
    {
        var stale = await context.Reviews
            .Where(r => r.Comment.Contains("Thandi"))
            .ToListAsync();

        if (stale.Count == 0)
            return;

        foreach (var review in stale)
            review.Comment = review.Comment.Replace("Thandi", "Zascia");

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a demonstration account in the given role, or returns the existing one.
    /// </summary>
    /// <param name="userManager">The Identity user manager.</param>
    /// <param name="email">The account email, also used as the username.</param>
    /// <param name="firstName">The account holder's first name.</param>
    /// <param name="lastName">The account holder's surname.</param>
    /// <param name="role">The role to assign.</param>
    /// <param name="bio">Optional host introduction shown on listing pages.</param>
    /// <param name="location">Optional home town shown beside the bio.</param>
    /// <param name="monthsOnPlatform">How long ago the account was created, in months.</param>
    /// <returns>The created or existing user, or <c>null</c> when creation failed.</returns>
    private static async Task<ApplicationUser?> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string firstName,
        string lastName,
        string role,
        string? bio = null,
        string? location = null,
        int monthsOnPlatform = 6)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            // Keep demo profiles in sync when seed names or bios change.
            existing.FirstName = firstName;
            existing.LastName = lastName;
            existing.Bio = bio;
            existing.Location = location;
            await userManager.UpdateAsync(existing);
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            Bio = bio,
            Location = location,
            // Staggered, but fixed rather than random, so a rebuilt database always
            // reproduces the same "hosting since" dates.
            CreatedAt = DateTime.UtcNow.AddMonths(-monthsOnPlatform)
        };

        var result = await userManager.CreateAsync(user, DemoPassword);
        if (!result.Succeeded)
            return null;

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
