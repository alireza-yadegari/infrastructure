using Payment.Dto;

namespace Payment.PaymentProvider;

internal class PayPalPaymentProvider : IPaymentProvider
{
  public string GetName() => "Paypal";

  public async Task<PaymentResponse> ProcessPayment(PaymentRequest request)
  {
    // Call PayPal API
  }

  public async Task<RefundResponse> ProcessRefund(RefundRequest request)
  {
    // Call PayPal API
  }

  public async Task<ProviderStatus> CheckStatus()
  {
    // Check PayPal status
  }
}