using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StayHub.Web;

/// <summary>
/// Renders enum members using their <see cref="DisplayAttribute"/> name.
/// </summary>
/// <remarks>
/// Several enums carry names that do not survive being written as identifiers -
/// "KwaZulu-Natal" and "Price: low to high" among them. Views call
/// <see cref="DisplayName"/> rather than <c>ToString</c> so those read correctly.
/// </remarks>
public static class EnumDisplayExtensions
{
    /// <summary>
    /// Gets the enum member's display name, falling back to its identifier.
    /// </summary>
    /// <param name="value">The enum member.</param>
    /// <returns>The display name.</returns>
    public static string DisplayName(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var name = value.ToString();

        return value.GetType()
                   .GetField(name, BindingFlags.Public | BindingFlags.Static)?
                   .GetCustomAttribute<DisplayAttribute>()?
                   .Name
               ?? name;
    }
}
