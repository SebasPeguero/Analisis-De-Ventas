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

public class ProductMasterLoader : IProductMasterLoader
{
    private readonly SistemaAnalisisVentasContext _context;

    public ProductMasterLoader(SistemaAnalisisVentasContext context)
    {
        _context = context;
    }

    public async Task<TableSummary> LoadCategoriesAsync(IEnumerable<ProductCsv> products)
    {
        var summary = new TableSummary { TableName = "Master.Categories" };
        var rawCategories = products
            .Select(p => p.Category?.Trim())
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        summary.Processed = rawCategories.Count;

        var existingCategories = await _context.Categories
            .ToDictionaryAsync(c => c.CategoryName, c => c.CategoryId, StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<Category>();
        foreach (var name in rawCategories)
        {
            if (!existingCategories.ContainsKey(name))
            {
                var category = new Category
                {
                    CategoryName = name,
                    Description = $"Categoria {name} importada"
                };
                toInsert.Add(category);
                summary.Inserted++;
            }
        }

        if (toInsert.Count > 0)
        {
            await _context.Categories.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();
        }

        return summary;
    }

    public async Task<TableSummary> LoadProductsAsync(IEnumerable<ProductCsv> products)
    {
        var summary = new TableSummary { TableName = "Master.Products" };
        var uniqueProducts = products
            .GroupBy(p => p.ProductId)
            .Select(g => g.First())
            .ToList();

        summary.Processed = uniqueProducts.Count;

        var categories = await _context.Categories
            .ToDictionaryAsync(c => c.CategoryName, c => c.CategoryId, StringComparer.OrdinalIgnoreCase);

        var existingProductCodes = new HashSet<string>(await _context.Products
            .Select(p => p.ProductCode)
            .Where(c => c != null)
            .ToListAsync());

        var toInsert = new List<Product>();
        foreach (var csv in uniqueProducts)
        {
            var code = csv.ProductId.ToString();
            if (existingProductCodes.Contains(code))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(csv.ProductName) || csv.Price < 0 || csv.Stock < 0)
            {
                summary.Rejected++;
                continue;
            }

            var categoryName = csv.Category?.Trim();
            if (string.IsNullOrEmpty(categoryName) || !categories.TryGetValue(categoryName, out int categoryId))
            {
                summary.Rejected++;
                continue;
            }

            var product = new Product
            {
                ProductCode = code,
                ProductName = csv.ProductName.Trim(),
                CategoryId = categoryId,
                UnitPrice = csv.Price,
                UnitsInStock = csv.Stock,
                UnitsInOrder = 0,
                Discontinued = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            toInsert.Add(product);
            summary.Inserted++;
        }

        if (toInsert.Count > 0)
        {
            await _context.Products.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();
        }

        return summary;
    }
}
