using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string Name { get; set; } = null!;

    public int? TableId { get; set; }

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public DateOnly? Date { get; set; }

    public virtual ICollection<CustomerTable> CustomerTables { get; } = new List<CustomerTable>();

    public virtual ICollection<Order> Orders { get; } = new List<Order>();

    public virtual Table? Table { get; set; }

    public virtual ICollection<WaitingList> WaitingLists { get; } = new List<WaitingList>();
}
