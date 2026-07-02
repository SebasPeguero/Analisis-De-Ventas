CREATE VIEW Fact.vw_SalesSummaryByCountryAndCategory AS
SELECT 
    dc.CountryName,
    dc.CityName,
    dp.CategoryName,
    dd.[Year],
    dd.MonthName,
    SUM(f.Quantity) AS TotalQuantity,
    AVG(f.UnitPrice) AS AverageUnitPrice,
    SUM(f.Total) AS TotalSales
FROM Fact.Sales f
INNER JOIN Dim.Customer dc ON f.CustomerKey = dc.CustomerKey
INNER JOIN Dim.Product dp ON f.ProductKey = dp.ProductKey
INNER JOIN Dim.Date dd ON f.DateKey = dd.DateKey
GROUP BY dc.CountryName, dc.CityName, dp.CategoryName, dd.[Year], dd.MonthName;
GO

CREATE PROCEDURE Fact.sp_GetCustomerPurchaseHistory
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        f.OrderID,
        dd.FullDate AS OrderDate,
        dp.ProductName,
        f.Quantity,
        f.UnitPrice,
        f.Total,
        ds.StatusName
    FROM Fact.Sales f
    INNER JOIN Dim.Customer dc ON f.CustomerKey = dc.CustomerKey
    INNER JOIN Dim.Product dp ON f.ProductKey = dp.ProductKey
    INNER JOIN Dim.Status ds ON f.StatusKey = ds.StatusKey
    INNER JOIN Dim.Date dd ON f.DateKey = dd.DateKey
    WHERE dc.Email = @Email
    ORDER BY dd.FullDate DESC;
END;
GO

SELECT 
    dp.CategoryName,
    SUM(f.Quantity) AS UnidadesVendidas,
    SUM(f.Total) AS IngresosTotales
FROM Fact.Sales f
INNER JOIN Dim.Product dp ON f.ProductKey = dp.ProductKey
GROUP BY dp.CategoryName
ORDER BY IngresosTotales DESC;
GO

SELECT TOP 10
    dc.CustomerID,
    dc.FullName,
    dc.Email,
    COUNT(DISTINCT f.OrderID) AS TotalOrdenes,
    SUM(f.Total) AS TotalGastado
FROM Fact.Sales f
INNER JOIN Dim.Customer dc ON f.CustomerKey = dc.CustomerKey
GROUP BY dc.CustomerID, dc.FullName, dc.Email
ORDER BY TotalGastado DESC;
GO

SELECT 
    ds.StatusName,
    COUNT(DISTINCT f.OrderID) AS TotalOrdenes,
    SUM(f.Total) AS TotalVentas,
    CAST(SUM(f.Total) * 100.0 / (SELECT SUM(Total) FROM Fact.Sales) AS DECIMAL(5,2)) AS PorcentajeVentas
FROM Fact.Sales f
INNER JOIN Dim.Status ds ON f.StatusKey = ds.StatusKey
GROUP BY ds.StatusName
ORDER BY TotalVentas DESC;
GO

SELECT 
    dc.CountryName,
    COUNT(DISTINCT dc.CustomerKey) AS CantidadClientes,
    SUM(f.Total) AS TotalFacturado
FROM Fact.Sales f
INNER JOIN Dim.Customer dc ON f.CustomerKey = dc.CustomerKey
GROUP BY dc.CountryName
ORDER BY TotalFacturado DESC;
GO

SELECT 
    dp.ProductID,
    dp.ProductName,
    dp.CategoryName,
    SUM(f.Quantity) AS CantidadVendida,
    mp.UnitsInStock AS StockActualEnBodega
FROM Fact.Sales f
INNER JOIN Dim.Product dp ON f.ProductKey = dp.ProductKey
INNER JOIN Master.Products mp ON dp.ProductID = mp.ProductID
GROUP BY dp.ProductID, dp.ProductName, dp.CategoryName, mp.UnitsInStock
ORDER BY CantidadVendida DESC;
GO
