using AdoNet_EfCore.Models.Entities.Sales;
﻿using System;
using System.Collections.Generic;

namespace AdoNet_EfCore.Models.Entities.Master;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal UnitPrice { get; set; }

    public int UnitsInStock { get; set; }

    public int UnitsInOrder { get; set; }

    public bool Discontinued { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}