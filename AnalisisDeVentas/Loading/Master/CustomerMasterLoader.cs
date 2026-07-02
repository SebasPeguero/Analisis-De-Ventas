using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnalisisDeVentas.Data;
using AnalisisDeVentas.Models;
using AnalisisDeVentas.Models.Csv;
using AnalisisDeVentas.Models.Entities.Master;
using Microsoft.EntityFrameworkCore;

namespace AnalisisDeVentas.Loading.Master;

public class CustomerMasterLoader : ICustomerMasterLoader
{
    private readonly SistemaAnalisisVentasContext _context;

    public CustomerMasterLoader(SistemaAnalisisVentasContext context)
    {
        _context = context;
    }

    public async Task<TableSummary> LoadCountriesAsync(IEnumerable<CustomerCsv> customers)
    {
        var summary = new TableSummary { TableName = "Master.Countries" };
        var rawCountries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in customers)
        {
            var country = c.Country?.Trim();
            if (!string.IsNullOrEmpty(country))
            {
                rawCountries.Add(country);
            }
        }

        summary.Processed = rawCountries.Count;

        var existingCountries = await _context.Countries
            .ToDictionaryAsync(c => c.CountryName, c => c.CountryId, StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<Country>();
        foreach (var countryName in rawCountries)
        {
            if (!existingCountries.ContainsKey(countryName))
            {
                var country = new Country { CountryName = countryName };
                toInsert.Add(country);
                summary.Inserted++;
            }
        }

        if (toInsert.Count > 0)
        {
            await _context.Countries.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();
        }

        return summary;
    }

    public async Task<TableSummary> LoadCitiesAsync(IEnumerable<CustomerCsv> customers)
    {
        var summary = new TableSummary { TableName = "Master.Cities" };
        var rawCities = new HashSet<(string City, string Country)>();

        foreach (var c in customers)
        {
            var city = c.City?.Trim();
            var country = c.Country?.Trim();
            if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(country))
            {
                rawCities.Add((city, country));
            }
        }

        summary.Processed = rawCities.Count;

        var countries = await _context.Countries
            .ToDictionaryAsync(c => c.CountryName, c => c.CountryId, StringComparer.OrdinalIgnoreCase);

        var existingCities = await _context.Cities
            .ToDictionaryAsync(c => $"{c.CityName}|{c.CountryId}", c => c.CityId, StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<City>();
        foreach (var item in rawCities)
        {
            if (!countries.TryGetValue(item.Country, out int countryId))
            {
                summary.Rejected++;
                continue;
            }

            var key = $"{item.City}|{countryId}";
            if (!existingCities.ContainsKey(key))
            {
                var city = new City { CityName = item.City, CountryId = countryId };
                toInsert.Add(city);
                summary.Inserted++;
            }
        }

        if (toInsert.Count > 0)
        {
            await _context.Cities.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();
        }

        return summary;
    }

    public async Task<TableSummary> LoadCustomersAsync(IEnumerable<CustomerCsv> customers)
    {
        var summary = new TableSummary { TableName = "Master.Customers" };
        
        var uniqueCustomers = new List<CustomerCsv>();
        var seenIds = new HashSet<int>();
        foreach (var c in customers)
        {
            if (seenIds.Add(c.CustomerId))
            {
                uniqueCustomers.Add(c);
            }
        }

        summary.Processed = uniqueCustomers.Count;

        var countries = await _context.Countries
            .ToDictionaryAsync(c => c.CountryName, c => c.CountryId, StringComparer.OrdinalIgnoreCase);

        var cities = await _context.Cities
            .ToDictionaryAsync(c => $"{c.CityName}|{c.CountryId}", c => c.CityId, StringComparer.OrdinalIgnoreCase);

        var existingCustomerCodes = new HashSet<string>(await _context.Customers
            .Select(c => c.CustomerCode)
            .ToListAsync());

        var toInsert = new List<Customer>();
        foreach (var csv in uniqueCustomers)
        {
            var code = csv.CustomerId.ToString();
            if (existingCustomerCodes.Contains(code))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(csv.FirstName) || string.IsNullOrWhiteSpace(csv.LastName) || string.IsNullOrWhiteSpace(csv.Email))
            {
                summary.Rejected++;
                continue;
            }

            if (!csv.Email.Contains("@"))
            {
                summary.Rejected++;
                continue;
            }

            var countryName = csv.Country?.Trim();
            var cityName = csv.City?.Trim();

            if (string.IsNullOrEmpty(countryName) || string.IsNullOrEmpty(cityName))
            {
                summary.Rejected++;
                continue;
            }

            if (!countries.TryGetValue(countryName, out int countryId))
            {
                summary.Rejected++;
                continue;
            }

            var cityKey = $"{cityName}|{countryId}";
            if (!cities.TryGetValue(cityKey, out int cityId))
            {
                summary.Rejected++;
                continue;
            }

            var customer = new Customer
            {
                CustomerCode = code,
                Email = csv.Email.Trim(),
                Phone = csv.Phone?.Trim(),
                CityId = cityId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            toInsert.Add(customer);
            summary.Inserted++;
        }

        if (toInsert.Count > 0)
        {
            await _context.Customers.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();
        }

        return summary;
    }
}
