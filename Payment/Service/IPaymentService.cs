using Payment.Dto;

namespace Payment.Service;

internal interface IPaymentService
{
  Task<List<string>> GetProviders();
    Task<PaymentResponse> ProcessPayment(string providerName, PaymentRequest request);
    Task<RefundResponse> ProcessRefund(string providerName, RefundRequest request);
    Task<ProviderStatus> CheckStatus(string providerName);
}