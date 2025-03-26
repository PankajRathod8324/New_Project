using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class WaitingList
{
    public int WaitingListId { get; set; }

    public DateTime WaitingTime { get; set; }

    public int? CustomerId { get; set; }

    public int? TableId { get; set; }

    public int? SectionId { get; set; }

    public string Name { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int NoOfPerson { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Section? Section { get; set; }

    public virtual Table? Table { get; set; }
}
