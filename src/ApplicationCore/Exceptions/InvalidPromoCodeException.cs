using System;

namespace Microsoft.eShopWeb.ApplicationCore.Exceptions;

public class InvalidPromoCodeException : Exception
{
    public InvalidPromoCodeException(string code)
        : base($"The promotion code '{code.Trim()}' is not recognized.")
    {
    }
}
