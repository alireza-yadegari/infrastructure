using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payment.Model;

[Table("PaymentDetail")]
internal class PaymentDetail
{
  [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public Guid Id { get; set; }

  [ForeignKey("Payment")]
  public Guid PaymentId { get; set; }
  public virtual Payment? Payment { get; set; }
  public required string Provider { get; set; }
  public decimal Amount { get; set; }
  public PaymentStatus Status { get; set; }
  public DateTime TimeStamp { get; set; }
}