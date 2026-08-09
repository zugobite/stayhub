namespace StayHub.Core.Enums;

/// <summary>
/// The listing categories a guest can filter the property gallery by.
/// </summary>
public enum PropertyType
{
    /// <summary>A self-contained flat.</summary>
    Apartment,

    /// <summary>A standalone house.</summary>
    House,

    /// <summary>A private or shared room within a larger dwelling.</summary>
    Room,

    /// <summary>A holiday villa.</summary>
    Villa,

    /// <summary>A rural or coastal cottage.</summary>
    Cottage,

    /// <summary>A guest house or bed and breakfast.</summary>
    GuestHouse
}
