using AnalisisDeVentas.Models.Entities.Sales;
﻿using System;
using System.Collections.Generic;

namespace AnalisisDeVentas.Models.Entities.Master;

public partial class City
{
    public int CityId { get; set; }

    public string CityName { get; set; } = null!;

    public int CountryId { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}