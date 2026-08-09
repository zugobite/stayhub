namespace StayHub.Web.Services;

/// <summary>
/// A card the sandbox gateway recognises, offered to the guest so the simulation
/// can be driven without inventing numbers.
/// </summary>
/// <param name="Number">The card number, in the gateway's spaced display form.</param>
/// <param name="Label">What this card demonstrates, e.g. "Approves immediately".</param>
/// <param name="Approves">Whether charging this card succeeds.</param>
public record SandboxCard(string Number, string Label, bool Approves);

/// <summary>
/// A request to charge a card.
/// </summary>
/// <param name="Amount">The amount in ZAR.</param>
/// <param name="CardNumber">The card number as typed, with or without spaces.</param>
/// <param name="CardHolder">The name on the card.</param>
/// <param name="Expiry">The expiry in MM/YY form.</param>
/// <param name="Cvv">The card security code.</param>
/// <param name="Description">What the charge is for, echoed onto the receipt.</param>
public record GatewayRequest(
    decimal Amount,
    string CardNumber,
    string CardHolder,
    string Expiry,
    string Cvv,
    string Description);

/// <summary>
/// The outcome of an authorisation attempt.
/// </summary>
/// <param name="Approved">Whether the charge succeeded.</param>
/// <param name="Reference">The gateway reference, present whether or not it succeeded.</param>
/// <param name="CardLast4">The last four digits of the card presented.</param>
/// <param name="FailureReason">Why the charge was declined, when it was.</param>
public record GatewayResult(
    bool Approved,
    string Reference,
    string CardLast4,
    string? FailureReason = null);

/// <summary>
/// Authorises card payments.
/// </summary>
/// <remarks>
/// The only implementation is a simulation. Isolating it behind an interface means
/// the booking flow is written against "some gateway" rather than against the
/// simulation, so swapping in a real provider would not touch the controller.
/// </remarks>
public interface IPaymentGateway
{
    /// <summary>Gets the gateway's display name, shown on the checkout page.</summary>
    string Name { get; }

    /// <summary>Gets the cards the sandbox recognises, listed on the checkout page.</summary>
    IReadOnlyList<SandboxCard> TestCards { get; }

    /// <summary>Gets the card pre-filled into the checkout form.</summary>
    SandboxCard DefaultCard { get; }

    /// <summary>
    /// Attempts to authorise a charge.
    /// </summary>
    /// <param name="request">The charge to attempt.</param>
    /// <returns>The gateway's decision.</returns>
    Task<GatewayResult> AuthoriseAsync(GatewayRequest request);
}
