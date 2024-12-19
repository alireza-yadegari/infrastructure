using Microsoft.AspNetCore.Mvc;
using Payment.Dto;
using Payment.Service;

namespace Payment.Api;

public static class PaymentEndpoints
{
  public static void MapAuthEndpoints(this WebApplication app)
  {
    app.MapPost("/api/v1/providers", async (string providerName, [FromServices] IPaymentService paymentService) =>
    {
      var response = await paymentService.GetProviders();
      return response;
    })
    .WithSummary("Get Providers")
    .WithTags("Payment")
    .WithOpenApi();

    app.MapPost("/api/v1/process", async (string providerName, PaymentRequest request, [FromServices] IPaymentService paymentService) =>
    {
      var response = await paymentService.ProcessPayment(providerName, request);
      return response;
    })
    .WithSummary("Payment Process")
    .WithTags("Payment")
    .WithOpenApi();

    app.MapPost("/api/v1/refund", async (string providerName, RefundRequest request, [FromServices] IPaymentService paymentService) =>
    {
      var response = await paymentService.ProcessRefund(providerName, request);
      return response;
    })
    .WithSummary("Process Refund")
    .WithTags("Payment")
    .WithOpenApi();

    app.MapPost("/api/v1/status", async (string providerName, [FromServices] IPaymentService paymentService) =>
    {
      var status = await paymentService.CheckStatus(providerName);
      return status;
    })
    .WithSummary("Provider Status")
    .WithTags("Payment")
    .WithOpenApi();
  }
}