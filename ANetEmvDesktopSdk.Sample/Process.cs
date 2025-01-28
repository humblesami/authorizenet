private async Task<Dictionary<string, string>> ProcessWithInPersonSDK(string amount, string orderId)
{
    try
    {
        // Call In-Person SDK's API or method
        var sdk = new AuthorizeNetSDK(); // Assuming SDK class exists
        var paymentResponse = await sdk.ProcessPaymentAsync(amount, orderId);

        if (paymentResponse.Success)
        {
            return new Dictionary<string, string>
            {
                { "transactionId", paymentResponse.TransactionId },
                { "status", "success" }
            };
        }
        else
        {
            return new Dictionary<string, string>
            {
                { "error", paymentResponse.ErrorMessage }
            };
        }
    }
    catch (Exception ex)
    {
        logger.log("Error interacting with SDK", ex);
        return new Dictionary<string, string> { { "error", "Failed to process payment with SDK." } };
    }
}

