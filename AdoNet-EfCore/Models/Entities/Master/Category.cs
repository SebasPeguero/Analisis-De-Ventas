using AdoNet_EfCore.Models.Entities.Sales;
﻿using System;
using System.Collections.Generic;

namespace AdoNet_EfCore.Models.Entities.Master;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}