using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.BasketTests;

public class BasketPromotion
{
    [Fact]
    public void AppliesPromo10IgnoringCaseAndSurroundingSpaces()
    {
        var basket = new Basket("buyer");
        basket.AddItem(1, 50.05m);

        basket.ApplyPromoCode("  promo10  ");

        Assert.Equal(PromotionCodes.Promo10, basket.PromoCode);
        Assert.Equal(50.05m, basket.Subtotal());
        Assert.Equal(5.01m, basket.DiscountAmount());
        Assert.Equal(45.04m, basket.Total());
    }

    [Fact]
    public void KeepsPromoCodeWithoutDiscountBelowMinimumAndRecalculatesAfterQuantityChange()
    {
        var basket = new Basket("buyer");
        basket.AddItem(1, 25m);
        basket.ApplyPromoCode("PROMO10");

        Assert.Equal(0m, basket.DiscountAmount());

        basket.AddItem(1, 25m);

        Assert.Equal(PromotionCodes.Promo10, basket.PromoCode);
        Assert.Equal(50m, basket.Subtotal());
        Assert.Equal(5m, basket.DiscountAmount());
        Assert.Equal(45m, basket.Total());
    }

    [Fact]
    public void CapsDiscountAtFifteenEuros()
    {
        var basket = new Basket("buyer");
        basket.AddItem(1, 200m);
        basket.ApplyPromoCode("PROMO10");

        Assert.Equal(15m, basket.DiscountAmount());
        Assert.Equal(185m, basket.Total());
    }

    [Fact]
    public void ApplyingSameCodeAgainDoesNotAccumulateDiscount()
    {
        var basket = new Basket("buyer");
        basket.AddItem(1, 100m);
        basket.ApplyPromoCode("PROMO10");
        var firstDiscount = basket.DiscountAmount();

        basket.ApplyPromoCode(" promo10 ");

        Assert.Equal(firstDiscount, basket.DiscountAmount());
        Assert.Equal(90m, basket.Total());
    }

    [Fact]
    public void RejectsUnknownCodeWithoutChangingExistingPromotion()
    {
        var basket = new Basket("buyer");
        basket.AddItem(1, 100m);
        basket.ApplyPromoCode("PROMO10");

        Assert.Throws<InvalidPromoCodeException>(() => basket.ApplyPromoCode("UNKNOWN"));
        Assert.Equal(PromotionCodes.Promo10, basket.PromoCode);
        Assert.Equal(10m, basket.DiscountAmount());
    }

    [Fact]
    public void RemovesPromotionCodeAndDiscount()
    {
        var basket = new Basket("buyer");
        basket.AddItem(1, 100m);
        basket.ApplyPromoCode("PROMO10");

        basket.RemovePromoCode();

        Assert.Null(basket.PromoCode);
        Assert.Equal(0m, basket.DiscountAmount());
        Assert.Equal(100m, basket.Total());
    }
}
