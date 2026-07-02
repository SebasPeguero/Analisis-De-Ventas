using AnalisisDeVentas.Models.Entities.Sales;
﻿using System;
using System.Collections.Generic;

namespace AnalisisDeVentas.Models.Entities.Master;

public partial class OrderStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}