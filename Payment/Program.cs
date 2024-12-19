using Microsoft.EntityFrameworkCore;
using Payment.DataAccess;
using Payment.PaymentProvider;
using Payment.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<PaymentDbContext>(options => options.UseSqlServer(builder.Configuration["Database:ConnectionString"].TrimStart('"').TrimEnd('"')));

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentProvider, PayPalPaymentProvider>();
builder.Services.AddScoped<IPaymentProvider, CreditCardPaymentProvider>();

var app = builder.Build();

app.Run();
