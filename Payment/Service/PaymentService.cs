using Payment.Dto;
using Payment.PaymentProvider;

namespace Payment.Service;

internal class PaymentService : IPaymentService
{
  private readonly IEnumerable<IPaymentProvider> _paymentProviders;

  public PaymentService(IEnumerable<IPaymentProvider> paymentProviders)
  {
    _paymentProviders = paymentProviders;
  }

  public async Task<List<string>> GetProviders()
  {
    return _paymentProviders.Select(x => x.GetName()).ToList();
  }

  public async Task<PaymentResponse> ProcessPayment(string providerName, PaymentRequest request)
  {
    var provider = _paymentProviders.FirstOrDefault(p => p.GetName().StartsWith(providerName, StringComparison.OrdinalIgnoreCase));
    if (provider == null) throw new InvalidOperationException("Payment provider not found.");

    return await provider.ProcessPayment(request);
  }

  public async Task<RefundResponse> ProcessRefund(string providerName, RefundRequest request)
  {
    var provider = _paymentProviders.FirstOrDefault(p => p.GetName().StartsWith(providerName, StringComparison.OrdinalIgnoreCase));
    if (provider == null) throw new InvalidOperationException("Payment provider not found.");

    return await provider.ProcessRefund(request);
  }

  public async Task<ProviderStatus> CheckStatus(string providerName)
  {
    var provider = _paymentProviders.FirstOrDefault(p => p.GetName().StartsWith(providerName, StringComparison.OrdinalIgnoreCase));
    if (provider == null) throw new InvalidOperationException("Payment provider not found.");

    return await provider.CheckStatus();
  }
}
