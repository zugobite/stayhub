using System.Globalization;

namespace StayHub.Web.Services;

/// <summary>
/// A stand-in card gateway. No network call is made and no money moves; the
/// decision is derived from the card number presented.
/// </summary>
/// <remarks>
/// The card numbers below are the sandbox numbers every payment provider publishes
/// for testing. They are not, and cannot be, real accounts: they exist so that
/// integrations can be exercised without touching a live card. Nothing beyond the
/// last four digits is ever stored or logged.
/// </remarks>
public class SimulatedPaymentGateway : IPaymentGateway
{
    private const string ApprovingCard = "4242 4242 4242 4242";
    private const string DecliningCard = "4000 0000 0000 0002";
    private const string InsufficientFundsCard = "4000 0000 0000 9995";

    /// <inheritdoc />
    public string Name => "StayPay Sandbox";

    /// <inheritdoc />
    public IReadOnlyList<SandboxCard> TestCards { get; } = new[]
    {
        new SandboxCard(ApprovingCard, "Approves immediately", true),
        new SandboxCard(DecliningCard, "Declined by issuer", false),
        new SandboxCard(InsufficientFundsCard, "Insufficient funds", false)
    };

    /// <inheritdoc />
    public SandboxCard DefaultCard => TestCards[0];

    /// <inheritdoc />
    public async Task<GatewayResult> AuthoriseAsync(GatewayRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var digits = new string((request.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        var last4 = digits.Length >= 4 ? digits[^4..] : digits;
        var reference = CreateReference();

        // A real gateway round-trips to the issuer; the pause stands in for that so
        // the interstitial "authorising" state is visible rather than instantaneous.
        await Task.Delay(400);

        if (digits.Length is < 13 or > 19 || !PassesLuhn(digits))
            return new GatewayResult(false, reference, last4, "That card number is not valid.");

        if (!IsExpiryValid(request.Expiry))
            return new GatewayResult(false, reference, last4, "That expiry date has passed or is malformed.");

        if (request.Cvv is null || request.Cvv.Length is < 3 or > 4 || !request.Cvv.All(char.IsDigit))
            return new GatewayResult(false, reference, last4, "The security code must be three or four digits.");

        if (string.IsNullOrWhiteSpace(request.CardHolder))
            return new GatewayResult(false, reference, last4, "The cardholder name is required.");

        if (request.Amount <= 0)
            return new GatewayResult(false, reference, last4, "The amount must be greater than zero.");

        var match = TestCards.FirstOrDefault(c => Normalise(c.Number) == digits);

        if (match is null)
        {
            return new GatewayResult(false, reference, last4,
                "Unrecognised card. Use one of the sandbox cards listed on this page.");
        }

        return match.Approves
            ? new GatewayResult(true, reference, last4)
            : new GatewayResult(false, reference, last4, $"Charge declined: {match.Label.ToLowerInvariant()}.");
    }

    private static string Normalise(string cardNumber) =>
        new(cardNumber.Where(char.IsDigit).ToArray());

    /// <summary>
    /// Creates a human-quotable reference, e.g. <c>SP-26A7F3K9</c>.
    /// </summary>
    private static string CreateReference()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var suffix = string.Concat(Enumerable.Range(0, 8)
            .Select(_ => alphabet[Random.Shared.Next(alphabet.Length)]));

        return $"SP-{suffix}";
    }

    /// <summary>
    /// Applies the Luhn checksum, the same modulus-10 test issuers use to catch
    /// mistyped card numbers before a request ever leaves the page.
    /// </summary>
    /// <param name="digits">The card number, digits only.</param>
    /// <returns><c>true</c> when the checksum is valid.</returns>
    private static bool PassesLuhn(string digits)
    {
        var sum = 0;
        var doubling = false;

        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var value = digits[i] - '0';

            if (doubling)
            {
                value *= 2;

                if (value > 9)
                    value -= 9;
            }

            sum += value;
            doubling = !doubling;
        }

        return sum % 10 == 0;
    }

    /// <summary>
    /// Checks an MM/YY expiry is well formed and has not passed.
    /// </summary>
    private static bool IsExpiryValid(string? expiry)
    {
        if (string.IsNullOrWhiteSpace(expiry))
            return false;

        var parts = expiry.Split('/', StringSplitOptions.TrimEntries);

        if (parts.Length != 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var year)
            || month is < 1 or > 12)
        {
            return false;
        }

        // A card is valid through the last day of its expiry month.
        var expiresAfter = new DateTime(2000 + year, month, 1).AddMonths(1).AddDays(-1);
        return expiresAfter >= DateTime.UtcNow.Date;
    }
}
