using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.PromotionRepositoryTests;

public class PromotionCodePersistence
{
    [Fact]
    public async Task PersistsBasketCodeAndOrderDiscountSnapshot()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase($"Promotion-{Guid.NewGuid()}")
            .Options;

        await using var context = new CatalogContext(options);
        var basket = new Basket("buyer");
        basket.AddItem(1, 100m);
        basket.ApplyPromoCode("PROMO10");
        context.Baskets.Add(basket);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.ChangeTracker.Clear();
        var loadedBasket = await context.Baskets
            .Include(item => item.Items)
            .SingleAsync(item => item.BuyerId == "buyer", TestContext.Current.CancellationToken);

        Assert.Equal(PromotionCodes.Promo10, loadedBasket.PromoCode);
        Assert.Equal(10m, loadedBasket.DiscountAmount());

        var order = new Order(
            loadedBasket.BuyerId,
            new Address("street", "city", "state", "country", "zip"),
            new List<OrderItem>
            {
                new(new CatalogItemOrdered(1, "product", "image.jpg"), 100m, 1)
            },
            loadedBasket.PromoCode,
            loadedBasket.DiscountAmount());
        context.Orders.Add(order);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        loadedBasket.AddItem(1, 100m);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.ChangeTracker.Clear();
        var loadedOrder = await context.Orders
            .Include(item => item.OrderItems)
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(PromotionCodes.Promo10, loadedOrder.PromoCode);
        Assert.Equal(10m, loadedOrder.DiscountAmount);
        Assert.Equal(90m, loadedOrder.Total());
    }
}
