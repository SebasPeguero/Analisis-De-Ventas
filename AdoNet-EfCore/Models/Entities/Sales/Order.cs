using AdoNet_EfCore.Models.Entities.Master;
﻿using System;
using System.Collections.Generic;

namespace AdoNet_EfCore.Models.Entities.Sales;

public partial class Order
{
    public int OrderId { get; set; }

    public string OrderCode { get; set; } = null!;

    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public int StatusId { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual OrderStatus Status { get; set; } = null!;
}
