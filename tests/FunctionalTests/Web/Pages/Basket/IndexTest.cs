using System.Globalization;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

[Collection("Sequential")]
public class IndexTest : IClassFixture<TestApplication>
{
    public IndexTest(TestApplication factory)
    {
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    public HttpClient Client { get; }


    [Fact]
    public async Task OnPostUpdateTo50Successfully()
    {
        // Load Home Page
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse1 = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        string token = WebPageHelpers.GetRequestVerificationToken(stringResponse1);

        // Add Item to Cart
        var keyValues = new List<KeyValuePair<string, string>>
        {
           new("id", "2"),
           new("name", "shirt"),
           new("__RequestVerificationToken", token)
        };
        var formContent = new FormUrlEncodedContent(keyValues);
        var postResponse = await Client.PostAsync("/basket/index", formContent, TestContext.Current.CancellationToken);
        postResponse.EnsureSuccessStatusCode();
        var stringResponse = await postResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(".NET Black &amp; White Mug", stringResponse);

        //Update
        var updateKeyValues = new List<KeyValuePair<string, string>>
        {
           new("Items[0].Id", WebPageHelpers.GetId(stringResponse)),
           new("Items[0].Quantity", "49"),
           new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(stringResponse))
        };
        var updateContent = new FormUrlEncodedContent(updateKeyValues);
        var updateResponse = await Client.PostAsync("/basket/update", updateContent, TestContext.Current.CancellationToken);

        var stringUpdateResponse = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("/basket/update", updateResponse!.RequestMessage!.RequestUri!.ToString()!);
        decimal expectedTotalAmount = 416.50M;
        Assert.Contains(expectedTotalAmount.ToString("N2"), stringUpdateResponse);
    }

    [Fact]
    public async Task OnPostUpdateTo0EmptyBasket()
    {
        // Load Home Page
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse1 = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        string token = WebPageHelpers.GetRequestVerificationToken(stringResponse1);

        // Add Item to Cart
        var keyValues = new List<KeyValuePair<string, string>>
        {
            new("id", "2"),
            new("name", "shirt"),
            new("__RequestVerificationToken", token)
        };
        var formContent = new FormUrlEncodedContent(keyValues);
        var postResponse = await Client.PostAsync("/basket/index", formContent, TestContext.Current.CancellationToken);
        postResponse.EnsureSuccessStatusCode();
        var stringResponse = await postResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(".NET Black &amp; White Mug", stringResponse);

        //Update
        var updateKeyValues = new List<KeyValuePair<string, string>>
        {
            new("Items[0].Id", WebPageHelpers.GetId(stringResponse)),
            new("Items[0].Quantity", "0"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(stringResponse))
        };
        var updateContent = new FormUrlEncodedContent(updateKeyValues);
        var updateResponse = await Client.PostAsync("/basket/update", updateContent, TestContext.Current.CancellationToken);

        var stringUpdateResponse = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("/basket/update", updateResponse!.RequestMessage!.RequestUri!.ToString()!);
        Assert.Contains("Basket is empty", stringUpdateResponse);
    }

    [Fact]
    public async Task AppliesAndRemovesPromotionCode()
    {
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var homeResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var addContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("id", "2"),
            new("__RequestVerificationToken", WebPageHelpers.GetRequestVerificationToken(homeResponse))
        });
        var addResponse = await Client.PostAsync("/basket/index", addContent, TestContext.Current.CancellationToken);
        var basketResponse = await addResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var updateContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("Items[0].Id", WebPageHelpers.GetId(basketResponse)),
            new("Items[0].Quantity", "6"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(basketResponse))
        });
        var updateResponse = await Client.PostAsync("/basket/update", updateContent, TestContext.Current.CancellationToken);
        var updatedBasket = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var applyContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("PromoCodeInput", " promo10 "),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(updatedBasket))
        });
        var applyResponse = await Client.PostAsync("/basket/applypromocode", applyContent, TestContext.Current.CancellationToken);
        var appliedBasket = await applyResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("PROMO10", appliedBasket);
        Assert.Contains(51m.ToString("N2", CultureInfo.CurrentCulture), appliedBasket);
        Assert.Contains(5.10m.ToString("N2", CultureInfo.CurrentCulture), appliedBasket);
        Assert.Contains(45.90m.ToString("N2", CultureInfo.CurrentCulture), appliedBasket);

        var invalidContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("PromoCodeInput", "UNKNOWN"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(appliedBasket))
        });
        var invalidResponse = await Client.PostAsync("/basket/applypromocode", invalidContent, TestContext.Current.CancellationToken);
        var invalidBasket = await invalidResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("not recognized", invalidBasket);
        Assert.Contains(45.90m.ToString("N2", CultureInfo.CurrentCulture), invalidBasket);

        var removeContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(invalidBasket))
        });
        var removeResponse = await Client.PostAsync("/basket/removepromocode", removeContent, TestContext.Current.CancellationToken);
        var removedBasket = await removeResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains(51m.ToString("N2", CultureInfo.CurrentCulture), removedBasket);
        Assert.DoesNotContain("Applied promotion", removedBasket);
    }

    [Fact]
    public async Task KeepsPromotionCodeAndDisplaysMinimumMessageBelowThreshold()
    {
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        var homeResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var addContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("id", "2"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(homeResponse))
        });
        var addResponse = await Client.PostAsync("/basket/index", addContent, TestContext.Current.CancellationToken);
        var basketResponse = await addResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var applyContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("PromoCodeInput", "PROMO10"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(basketResponse))
        });
        var applyResponse = await Client.PostAsync("/basket/applypromocode", applyContent, TestContext.Current.CancellationToken);
        var appliedBasket = await applyResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("PROMO10", appliedBasket);
        Assert.Contains("minimum purchase of €50.00", appliedBasket);
        Assert.Contains(0m.ToString("N2", CultureInfo.CurrentCulture), appliedBasket);
    }
}
