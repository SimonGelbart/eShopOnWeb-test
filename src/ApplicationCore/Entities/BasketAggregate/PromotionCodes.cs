namespace Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

public static class PromotionCodes
{
    public const string Promo10 = "PROMO10";
    public const decimal MinimumSubtotal = 50m;
    public const decimal DiscountRate = 0.10m;
    public const decimal MaximumDiscount = 15m;

    public static bool TryNormalize(string? code, out string normalizedCode)
    {
        normalizedCode = code?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalizedCode == Promo10;
    }
}
