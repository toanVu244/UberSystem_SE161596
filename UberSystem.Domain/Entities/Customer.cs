using System;
using System.Collections.Generic;

namespace UberSystem.Domain.Entities;

public partial class Customer
{
    public long Id { get; set; }

    public byte[] CreateAt { get; set; } = null!;

    public long? UserId { get; set; }

    public virtual ICollection<Rating> Ratings { get; } = new List<Rating>();

    public virtual ICollection<Trip> Trips { get; } = new List<Trip>();

    public virtual User? User { get; set; }
}
