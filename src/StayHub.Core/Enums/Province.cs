using System.ComponentModel.DataAnnotations;

namespace StayHub.Core.Enums;

/// <summary>
/// The nine South African provinces a listing can be located in. Used by the
/// gallery's region filter, which is coarser than the free-text city filter.
/// </summary>
public enum Province
{
    /// <summary>Western Cape.</summary>
    [Display(Name = "Western Cape")]
    WesternCape,

    /// <summary>Eastern Cape.</summary>
    [Display(Name = "Eastern Cape")]
    EasternCape,

    /// <summary>Northern Cape.</summary>
    [Display(Name = "Northern Cape")]
    NorthernCape,

    /// <summary>KwaZulu-Natal.</summary>
    [Display(Name = "KwaZulu-Natal")]
    KwaZuluNatal,

    /// <summary>Gauteng.</summary>
    [Display(Name = "Gauteng")]
    Gauteng,

    /// <summary>Free State.</summary>
    [Display(Name = "Free State")]
    FreeState,

    /// <summary>Mpumalanga.</summary>
    [Display(Name = "Mpumalanga")]
    Mpumalanga,

    /// <summary>Limpopo.</summary>
    [Display(Name = "Limpopo")]
    Limpopo,

    /// <summary>North West.</summary>
    [Display(Name = "North West")]
    NorthWest
}
