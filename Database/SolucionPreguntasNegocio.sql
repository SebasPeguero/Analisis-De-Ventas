USE SistemaAnalisisVentas;
GO

SELECT SUM(Total) FROM Fact.Sales;
GO

SELECT AVG(TotalOrden) FROM (SELECT OrderID, SUM(Total) AS TotalOrden FROM Fact.Sales GROUP BY OrderID) t;
GO

SELECT SUM(f.Total) FROM Fact.Sales f INNER JOIN Dim.Date d ON f.DateKey = d.DateKey WHERE d.Year = 2024;
GO

SELECT c.CountryName, c.RegionName, c.CityName, SUM(f.Total) AS TotalVentas FROM Fact.Sales f INNER JOIN Dim.Customer c ON f.CustomerKey = c.CustomerKey GROUP BY c.CountryName, c.RegionName, c.CityName;
GO

SELECT TOP 10 p.ProductName, SUM(f.Quantity) AS TotalUnidades FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey INNER JOIN Dim.Date d ON f.DateKey = d.DateKey WHERE d.Year = 2024 GROUP BY p.ProductName ORDER BY TotalUnidades DESC;
GO

SELECT TOP 10 p.ProductName, SUM(f.Total) AS TotalVentas FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey GROUP BY p.ProductName ORDER BY TotalVentas DESC;
GO

SELECT TOP 10 p.ProductName, SUM(f.Quantity) AS TotalUnidades FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey GROUP BY p.ProductName ORDER BY TotalUnidades ASC;
GO

SELECT d.Year, d.Month, d.MonthName, SUM(f.Quantity) AS UnidadesVendidas FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey INNER JOIN Dim.Date d ON f.DateKey = d.DateKey WHERE p.ProductID = 1 GROUP BY d.Year, d.Month, d.MonthName ORDER BY d.Year, d.Month;
GO

SELECT p.ProductName, AVG(f.UnitPrice) AS PrecioPromedio FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey GROUP BY p.ProductName;
GO

SELECT TOP 10 c.FullName, COUNT(DISTINCT f.OrderID) AS CantidadOrdenes FROM Fact.Sales f INNER JOIN Dim.Customer c ON f.CustomerKey = c.CustomerKey GROUP BY c.FullName ORDER BY CantidadOrdenes DESC;
GO

SELECT TOP 10 c.FullName, SUM(f.Total) AS TotalVentas FROM Fact.Sales f INNER JOIN Dim.Customer c ON f.CustomerKey = c.CustomerKey GROUP BY c.FullName ORDER BY TotalVentas DESC;
GO

SELECT AVG(CantidadProductos) FROM (SELECT OrderID, CustomerKey, SUM(Quantity) AS CantidadProductos FROM Fact.Sales GROUP BY OrderID, CustomerKey) t;
GO

WITH TopClientes AS (SELECT TOP 5 CustomerKey, SUM(Total) AS VentasCliente FROM Fact.Sales GROUP BY CustomerKey ORDER BY VentasCliente DESC) SELECT SUM(tc.VentasCliente) * 100.0 / (SELECT SUM(Total) FROM Fact.Sales) AS PorcentajeTop5 FROM TopClientes tc;
GO

SELECT c.Segment, c.CountryName, COUNT(DISTINCT f.OrderID) AS TotalOrdenes, SUM(f.Total) AS TotalVentas FROM Fact.Sales f INNER JOIN Dim.Customer c ON f.CustomerKey = c.CustomerKey GROUP BY c.Segment, c.CountryName;
GO

SELECT d.Year, d.Quarter, d.Month, d.MonthName, SUM(f.Total) AS TotalVentas FROM Fact.Sales f INNER JOIN Dim.Date d ON f.DateKey = d.DateKey GROUP BY d.Year, d.Quarter, d.Month, d.MonthName ORDER BY d.Year, d.Month;
GO

SELECT d.MonthName, SUM(f.Total) AS TotalVentas FROM Fact.Sales f INNER JOIN Dim.Date d ON f.DateKey = d.DateKey GROUP BY d.MonthName ORDER BY TotalVentas DESC;
GO

SELECT p.ProductName, d.Quarter, SUM(f.Quantity) AS UnidadesVendidas FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey INNER JOIN Dim.Date d ON f.DateKey = d.DateKey GROUP BY p.ProductName, d.Quarter ORDER BY p.ProductName, d.Quarter;
GO

SELECT d.Year, d.Month, d.MonthName, SUM(f.Total) AS TotalVentas FROM Fact.Sales f INNER JOIN Dim.Date d ON f.DateKey = d.DateKey GROUP BY d.Year, d.Month, d.MonthName ORDER BY d.Year, d.Month;
GO

SELECT p.CategoryName, SUM(f.Total) AS TotalVentas FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey GROUP BY p.CategoryName;
GO

SELECT p.CategoryName, SUM(f.Total) AS TotalCategoria, SUM(f.Total) * 100.0 / (SELECT SUM(Total) FROM Fact.Sales) AS PorcentajeVentas FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey GROUP BY p.CategoryName;
GO

SELECT v.SellerName, v.SellerRegion, SUM(f.Total) AS VentasVendedor FROM Fact.Sales f INNER JOIN Dim.Seller v ON f.SellerKey = v.SellerKey GROUP BY v.SellerName, v.SellerRegion ORDER BY VentasVendedor DESC;
GO

SELECT d.Month, d.MonthName, SUM(CASE WHEN d.Year = 2024 THEN f.Total ELSE 0 END) AS Ventas2024, SUM(CASE WHEN d.Year = 2023 THEN f.Total ELSE 0 END) AS Ventas2023 FROM Fact.Sales f INNER JOIN Dim.Date d ON f.DateKey = d.DateKey GROUP BY d.Month, d.MonthName ORDER BY d.Month;
GO

SELECT SUM(Total) FROM Fact.Sales;
GO

SELECT p.ProductName, c.FullName, d.Year, d.MonthName, SUM(f.Total) AS VentasTotales FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey INNER JOIN Dim.Customer c ON f.CustomerKey = c.CustomerKey INNER JOIN Dim.Date d ON f.DateKey = d.DateKey GROUP BY p.ProductName, c.FullName, d.Year, d.MonthName;
GO

SELECT TOP 5 p.ProductName, SUM(f.Quantity) AS UnidadesVendidas FROM Fact.Sales f INNER JOIN Dim.Product p ON f.ProductKey = p.ProductKey GROUP BY p.ProductName ORDER BY UnidadesVendidas DESC;
GO

SELECT TOP 5 c.FullName, COUNT(DISTINCT f.OrderID) AS TotalOrdenes FROM Fact.Sales f INNER JOIN Dim.Customer c ON f.CustomerKey = c.CustomerKey GROUP BY c.FullName ORDER BY TotalOrdenes DESC;
GO

SELECT c.FullName, AVG(f.Total) AS VentaPromedio FROM Fact.Sales f INNER JOIN Dim.Customer c ON f.CustomerKey = c.CustomerKey GROUP BY c.FullName;
GO

WITH VentasMensuales AS (SELECT d.Year, d.Month, SUM(f.Total) AS VentasTotal, LAG(SUM(f.Total)) OVER (ORDER BY d.Year, d.Month) AS VentasMesAnterior FROM Fact.Sales f INNER JOIN Dim.Date d ON f.DateKey = d.DateKey GROUP BY d.Year, d.Month) SELECT Year, Month, VentasTotal, VentasMesAnterior, (VentasTotal - VentasMesAnterior) * 100.0 / VentasMesAnterior AS CrecimientoPorcentual FROM VentasMensuales;
GO
