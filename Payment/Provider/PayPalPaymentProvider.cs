using Payment.Dto;

namespace Payment.PaymentProvider;

internal class PayPalPaymentProvider : IPaymentProvider
{
  public string GetName() => "Paypal";

  public async Task<PaymentResponse> ProcessPayment(PaymentRequest request)
  {
    throw new NotImplementedException();
  }

  public async Task<RefundResponse> ProcessRefund(RefundRequest request)
  {
    throw new NotImplementedException();
  }

  public async Task<ProviderStatus> CheckStatus()
  {
    throw new NotImplementedException();
  }
}