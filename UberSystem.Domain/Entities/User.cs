using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace UberSystem.Domain.Entities;

public partial class User
{
    public long Id { get; set; }

    public string? Role { get; set; }

    public string? UserName { get; set; }

    public string Email { get; set; } = null!;
    public string? Password { get; set; }
    public bool Status { get; set; }  
    public string Code { get; set; }

    public virtual ICollection<Customer> Customers { get; } = new List<Customer>();

    public virtual ICollection<Driver> Drivers { get; } = new List<Driver>();

    public RefreshToken RefreshToken { get; set; }
}
