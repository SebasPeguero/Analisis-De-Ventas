using System;
using System.Threading.Tasks;
using AnalisisDeVentas.Data;
using Microsoft.Data.SqlClient;

namespace AnalisisDeVentas.Loading.Dimensional;

public class DimensionBuilder : IDimensionBuilder
{
    private readonly AdoConnectionFactory _connectionFactory;

    public DimensionBuilder(AdoConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task BuildDimensionsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "TRUNCATE TABLE [Fact].Sales;";
            await cmd.ExecuteNonQueryAsync();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "DELETE FROM Dim.Customer; DBCC CHECKIDENT ('Dim.Customer', RESEED, 0);";
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = @"
                INSERT INTO Dim.Customer (CustomerID, FullName, Email, CityName, CountryName)
                SELECT 
                    c.CustomerID,
                    TRIM(stg.FirstName + ' ' + stg.LastName) AS FullName,
                    c.Email,
                    ci.CityName,
                    co.CountryName
                FROM Master.Customers c
                INNER JOIN Master.Cities ci ON c.CityID = ci.CityID
                INNER JOIN Master.Countries co ON ci.CountryID = co.CountryID
                INNER JOIN Stg.Customers stg ON c.CustomerCode = CAST(stg.CustomerID AS NVARCHAR(50));
            ";
            await cmd.ExecuteNonQueryAsync();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "DELETE FROM Dim.Product; DBCC CHECKIDENT ('Dim.Product', RESEED, 0);";
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = @"
                INSERT INTO Dim.Product (ProductID, ProductName, CategoryName)
                SELECT 
                    p.ProductID,
                    p.ProductName,
                    cat.CategoryName
                FROM Master.Products p
                INNER JOIN Master.Categories cat ON p.CategoryID = cat.CategoryID;
            ";
            await cmd.ExecuteNonQueryAsync();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "DELETE FROM Dim.Status; DBCC CHECKIDENT ('Dim.Status', RESEED, 0);";
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = @"
                INSERT INTO Dim.Status (StatusID, StatusName)
                SELECT StatusID, StatusName FROM Master.OrderStatus;
            ";
            await cmd.ExecuteNonQueryAsync();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "DELETE FROM Dim.Date;";
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = @"
                INSERT INTO Dim.Date (DateKey, FullDate, [Year], [Month], MonthName, [Quarter])
                SELECT DISTINCT
                    YEAR(OrderDate) * 10000 + MONTH(OrderDate) * 100 + DAY(OrderDate) AS DateKey,
                    CAST(OrderDate AS DATE) AS FullDate,
                    YEAR(OrderDate) AS [Year],
                    MONTH(OrderDate) AS [Month],
                    DATENAME(month, OrderDate) AS MonthName,
                    DATEPART(quarter, OrderDate) AS [Quarter]
                FROM Sales.Orders;
            ";
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task BuildFactsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE [Fact].Sales;";
        await cmd.ExecuteNonQueryAsync();

        cmd.CommandText = @"
            INSERT INTO [Fact].Sales (OrderID, OrderDetailID, CustomerKey, ProductKey, StatusKey, DateKey, Quantity, UnitPrice, Total)
            SELECT 
                o.OrderID,
                d.OrderDetailID,
                dc.CustomerKey,
                dp.ProductKey,
                ds.StatusKey,
                YEAR(o.OrderDate) * 10000 + MONTH(o.OrderDate) * 100 + DAY(o.OrderDate) AS DateKey,
                d.Quantity,
                d.UnitPrice,
                d.Total
            FROM Sales.OrderDetails d
            INNER JOIN Sales.Orders o ON d.OrderID = o.OrderID
            INNER JOIN Master.Customers mc ON o.CustomerID = mc.CustomerID
            INNER JOIN Dim.Customer dc ON mc.CustomerID = dc.CustomerID
            INNER JOIN Master.Products mp ON d.ProductID = mp.ProductID
            INNER JOIN Dim.Product dp ON mp.ProductID = dp.ProductID
            INNER JOIN Dim.Status ds ON o.StatusID = ds.StatusID;
        ";
        await cmd.ExecuteNonQueryAsync();
    }
}
