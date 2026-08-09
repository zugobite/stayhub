using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Core.Models;

namespace StayHub.Data.Configurations;

/// <summary>
/// Maps newsletter subscription rows with a unique email index.
/// </summary>
public class NewsletterSubscriptionConfiguration : IEntityTypeConfiguration<NewsletterSubscription>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NewsletterSubscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(s => s.Email).IsUnique();
    }
}
