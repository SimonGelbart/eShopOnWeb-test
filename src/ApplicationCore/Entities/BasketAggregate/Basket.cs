using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

public class Basket : BaseEntity, IAggregateRoot
{
    public string BuyerId { get; private set; }
    public string? PromoCode { get; private set; }
    private readonly List<BasketItem> _items = new List<BasketItem>();
    public IReadOnlyCollection<BasketItem> Items => _items.AsReadOnly();

    public int TotalItems => _items.Sum(i => i.Quantity);


    public Basket(string buyerId)
    {
        BuyerId = buyerId;
    }

    public void AddItem(int catalogItemId, decimal unitPrice, int quantity = 1)
    {
        if (!Items.Any(i => i.CatalogItemId == catalogItemId))
        {
            _items.Add(new BasketItem(catalogItemId, quantity, unitPrice));
            return;
        }
        var existingItem = Items.First(i => i.CatalogItemId == catalogItemId);
        existingItem.AddQuantity(quantity);
    }

    public void RemoveEmptyItems()
    {
        _items.RemoveAll(i => i.Quantity == 0);
    }

    public void SetNewBuyerId(string buyerId)
    {
        BuyerId = buyerId;
    }

    public void ApplyPromoCode(string promoCode)
    {
        if (!PromotionCodes.TryNormalize(promoCode, out var normalizedCode))
        {
            throw new InvalidPromoCodeException(promoCode ?? string.Empty);
        }

        PromoCode = normalizedCode;
    }

    public void RemovePromoCode()
    {
        PromoCode = null;
    }

    public decimal Subtotal()
    {
        return RoundMoney(_items.Sum(i => i.UnitPrice * i.Quantity));
    }

    public decimal DiscountAmount()
    {
        if (PromoCode != PromotionCodes.Promo10 || Subtotal() < PromotionCodes.MinimumSubtotal)
        {
            return 0m;
        }

        var discount = RoundMoney(Subtotal() * PromotionCodes.DiscountRate);
        return Math.Min(discount, PromotionCodes.MaximumDiscount);
    }

    public decimal Total()
    {
        return RoundMoney(Subtotal() - DiscountAmount());
    }

    private static decimal RoundMoney(decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
