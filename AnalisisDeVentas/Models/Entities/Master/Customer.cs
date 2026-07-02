using AnalisisDeVentas.Models.Entities.Sales;
﻿using System;
using System.Collections.Generic;

namespace AnalisisDeVentas.Models.Entities.Master;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int CityId { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}