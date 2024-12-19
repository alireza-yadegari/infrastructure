using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Payment.Model;

[Table("Payment")]
internal class Payment
{
  [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public Guid Id { get; set; }
  public Guid TransactionId { get; set; }
  public required string Provider { get; set; }
  
  [Precision(18, 2)]
  public decimal Amount { get; set; }
  public PaymentStatus Status { get; set; }
  public DateTime TimeStamp { get; set; }
}