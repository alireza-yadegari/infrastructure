using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Model;

  [Table("User")]
  public class User : IdentityUser<Guid>
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override Guid Id { get; set; }

    public required string Name { get; set; }

    public string? LastName { get; set; }

    public bool Deleted { get; set; }

    public required string Password { get; set; }

    public string? EmailConfirmationCode { get; set; }

    public bool? ChangePassword { get; set; }
  }