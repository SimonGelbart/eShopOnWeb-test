using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.UnitTests.Builders;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.OrderTests;

public class OrderTotal
{
    private readonly decimal _testUnitPrice = 42m;

    [Fact]
    public void IsZeroForNewOrder()
    {
        var order = new OrderBuilder().WithNoItems();

        Assert.Equal(0, order.Total());
    }

    [Fact]
    public void IsCorrectGiven1Item()
    {
        var builder = new OrderBuilder();
        var items = new List<OrderItem>
            {
                new(builder.TestCatalogItemOrdered, _testUnitPrice, 1)
            };
        var order = new OrderBuilder().WithItems(items);
        Assert.Equal(_testUnitPrice, order.Total());
    }

    [Fact]
    public void IsCorrectGiven3Items()
    {
        var builder = new OrderBuilder();
        var order = builder.WithDefaultValues();

        Assert.Equal(builder.TestUnitPrice * builder.TestUnits, order.Total());
    }

    [Fact]
    public void AppliesSnapshotPromotionDiscountToTotal()
    {
        var builder = new OrderBuilder();
        var items = new List<OrderItem>
        {
            new(builder.TestCatalogItemOrdered, 100m, 1)
        };

        var order = new OrderBuilder().WithItems(items);
        order = new Order(builder.TestBuyerId, new AddressBuilder().WithDefaultValues(),
            items, " promo10 ", 10m);

        Assert.Equal("PROMO10", order.PromoCode);
        Assert.Equal(100m, order.Subtotal());
        Assert.Equal(10m, order.DiscountAmount);
        Assert.Equal(90m, order.Total());
    }
}
