using Microsoft.AspNetCore.Authorization.Infrastructure;
using Payment.Dto;

namespace Payment.PaymentProvider;

internal interface IPaymentProvider
{
  string GetName();
  Task<PaymentResponse> ProcessPayment(PaymentRequest request);
  Task<RefundResponse> ProcessRefund(RefundRequest request);
  Task<ProviderStatus> CheckStatus();
}