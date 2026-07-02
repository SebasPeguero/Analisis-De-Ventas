using AdoNet_EfCore.Models.Entities.Sales;
﻿using System;
using System.Collections.Generic;

namespace AdoNet_EfCore.Models.Entities.Master;

public partial class Country
{
    public int CountryId { get; set; }

    public string CountryName { get; set; } = null!;

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
