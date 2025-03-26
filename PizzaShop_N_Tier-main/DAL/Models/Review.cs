using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public int? Food { get; set; }

    public int? Service { get; set; }

    public int? Ambience { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
