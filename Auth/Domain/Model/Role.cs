using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Model;

  public class Role : IdentityRole<Guid>
  {
    public Role()
    {
    }

    public Role(string role)
    {
      this.Name = role;
    }
  }