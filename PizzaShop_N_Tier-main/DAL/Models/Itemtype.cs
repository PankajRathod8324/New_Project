using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Itemtype
{
    public int ItemtypeId { get; set; }

    public string ItemType1 { get; set; } = null!;

    public string? ItemPhoto { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; } = new List<MenuItem>();
}
