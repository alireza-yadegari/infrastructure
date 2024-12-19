using Microsoft.EntityFrameworkCore;

namespace Payment.DataAccess;

internal class PaymentDbContext : DbContext
{
  public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
      : base(options)
  {

  }

  internal DbSet<Model.Payment> payments { get; set; }
  internal DbSet<Model.PaymentDetail> paymentDetails { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
  }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);
  }
}


