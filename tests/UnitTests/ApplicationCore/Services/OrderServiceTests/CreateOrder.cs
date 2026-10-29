using Mediator;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.OrderServiceTests;

public class CreateOrder
{
    [Fact]
    public async Task CopiesPromotionCodeAndDiscountFromBasket()
    {
        var basket = new Basket("buyer");
        basket.AddItem(1, 100m);
        basket.ApplyPromoCode("PROMO10");

        var basketRepository = Substitute.For<IRepository<Basket>>();
        basketRepository.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(basket);

        var catalogItem = Substitute.For<CatalogItem>(1, 1, "description", "product", 100m, "image.jpg");
        catalogItem.Id.Returns(1);
        var catalogRepository = Substitute.For<IRepository<CatalogItem>>();
        catalogRepository.ListAsync(Arg.Any<CatalogItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<CatalogItem> { catalogItem });

        var orderRepository = Substitute.For<IRepository<Order>>();
        var uriComposer = Substitute.For<IUriComposer>();
        uriComposer.ComposePicUri(Arg.Any<string>()).Returns("https://example.test/image.jpg");
        var mediator = Substitute.For<IMediator>();
        var orderService = new OrderService(basketRepository, catalogRepository, orderRepository, uriComposer, mediator);

        await orderService.CreateOrderAsync(1, new Address("street", "city", "state", "country", "zip"));

        await orderRepository.Received().AddAsync(
            Arg.Is<Order>(order =>
                order != null &&
                order.PromoCode == PromotionCodes.Promo10 &&
                order.DiscountAmount == 10m &&
                order.Subtotal() == 100m &&
                order.Total() == 90m),
            Arg.Any<CancellationToken>());
    }
}
