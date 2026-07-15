# Resumen de Proyecto y Modelado de Base de Datos - Actividad 3.1

Este documento consolida la arquitectura, el diseño y las consultas SQL correspondientes a la Actividad 3.1 del Sistema de Análisis de Ventas (Data Warehouse).

---

## 1. Directrices y Preguntas del Negocio a Responder

El modelo de datos analítico (Data Warehouse) en estrella está diseñado estructuralmente para responder a las preguntas planteadas en la práctica:

### 1.1. Análisis General de Ventas
*   Total de ventas global registrado.
*   Promedio de ventas por transacción.
*   Ventas totales en periodos de tiempo específicos (día, mes, año).
*   Volumen de ventas por geografía (país, región, ciudad).

### 1.2. Ventas por Producto
*   Productos más vendidos y productos con menor rotación.
*   Productos que generan mayor ingreso total.
*   Evolución temporal de la demanda y precio promedio de venta.

### 1.3. Ventas por Cliente
*   Clientes con mayor número de compras e ingresos totales generados.
*   Promedio de productos por transacción.
*   Porcentaje de ventas perteneciente al Top 5 de clientes.
*   Segmentación comercial y geográfica del comportamiento de compras.

### 1.4. Tendencias Temporales
*   Tendencia de ventas mensual y trimestral.
*   Picos de venta y estacionalidad de productos.
*   Evolución del ingreso anual.

### 1.5. Comparativas y Desempeño
*   Porcentaje de participación e ingresos por categoría de producto.
*   Desempeño de vendedores y de regiones.
*   Comparación de ventas del año actual frente al año anterior.

---

## 2. Decisiones de Diseño de la Base de Datos

### 2.1. Selección del Modelo Estrella (Star Schema)
Se implementó un modelo en estrella en el esquema `Dim` y `Fact` para optimizar las consultas OLAP:
*   **Desnormalización:** Las dimensiones contienen atributos descriptivos directos (como país y ciudad en `Dim.Customer`), eliminando joins complejos.
*   **Velocidad de Agregación:** La tabla de hechos `Fact.Sales` almacena métricas cuantitativas clave y llaves foráneas directas, optimizando sumas y promedios.

### 2.2. Análisis de Gobernanza y Brecha de Datos (Seller Gap)
*   **Dimensión Vendedor (`Dim.Seller`):** Se incluyó en el diseño físico y lógico para soportar el análisis de rendimiento de vendedores requerido por el negocio.
*   **Justificación de Carga:** Dado que los archivos CSV origen no proveen información de vendedores, la tabla queda declarada en la base de datos pero vacía para la carga actual, lista para recibir datos de futuras integraciones (API o BD externa).

---

## 3. Diagrama Entidad-Relación (DER) en Estrella

El modelo analítico responde a la siguiente estructura lógica:

```
                  +-------------------+
                  |   Dim.Product     |
                  +-------------------+
                            | 1
                            |
                            | N
                  +-------------------+
                  |    Fact.Sales     |
                  +-------------------+
                 /          |          \
              N /         N |           \ N
               /            |            \
    +------------+    +------------+    +------------+
    |  Dim.Date  |    |Dim.Customer|    | Dim.Seller |
    +------------+    +------------+    +------------+
                            | N
                            |
                            | 1
                      +------------+
                      | Dim.Status |
                      +------------+
```

---

## 4. Script DDL de Creación (`CreateStarSchema.sql`)

```sql
USE SistemaAnalisisVentas;
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Dim')
BEGIN
    EXEC('CREATE SCHEMA Dim')
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Fact')
BEGIN
    EXEC('CREATE SCHEMA Fact')
END
GO

IF OBJECT_ID('Fact.Sales', 'U') IS NOT NULL DROP TABLE Fact.Sales;
IF OBJECT_ID('Dim.Customer', 'U') IS NOT NULL DROP TABLE Dim.Customer;
IF OBJECT_ID('Dim.Product', 'U') IS NOT NULL DROP TABLE Dim.Product;
IF OBJECT_ID('Dim.Seller', 'U') IS NOT NULL DROP TABLE Dim.Seller;
IF OBJECT_ID('Dim.Status', 'U') IS NOT NULL DROP TABLE Dim.Status;
IF OBJECT_ID('Dim.Date', 'U') IS NOT NULL DROP TABLE Dim.Date;
GO

CREATE TABLE Dim.Customer (
    CustomerKey INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    CityName NVARCHAR(100) NOT NULL,
    RegionName NVARCHAR(100) NOT NULL,
    CountryName NVARCHAR(100) NOT NULL,
    Segment NVARCHAR(50) NOT NULL
);

CREATE TABLE Dim.Product (
    ProductKey INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL,
    ProductName NVARCHAR(150) NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    StandardPrice DECIMAL(18,2) NOT NULL
);

CREATE TABLE Dim.Seller (
    SellerKey INT IDENTITY(1,1) PRIMARY KEY,
    SellerID INT NOT NULL,
    SellerName NVARCHAR(150) NOT NULL,
    SellerRegion NVARCHAR(100) NOT NULL
);

CREATE TABLE Dim.Status (
    StatusKey INT IDENTITY(1,1) PRIMARY KEY,
    StatusID INT NOT NULL,
    StatusName NVARCHAR(50) NOT NULL
);

CREATE TABLE Dim.Date (
    DateKey INT PRIMARY KEY,
    FullDate DATE NOT NULL,
    Year INT NOT NULL,
    Month INT NOT NULL,
    MonthName NVARCHAR(20) NOT NULL,
    Quarter INT NOT NULL
);

CREATE TABLE Fact.Sales (
    SalesKey INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    OrderDetailID INT NOT NULL,
    CustomerKey INT NOT NULL,
    ProductKey INT NOT NULL,
    SellerKey INT NOT NULL,
    StatusKey INT NOT NULL,
    DateKey INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_Fact_Sales_Customer FOREIGN KEY (CustomerKey) REFERENCES Dim.Customer(CustomerKey),
    CONSTRAINT FK_Fact_Sales_Product FOREIGN KEY (ProductKey) REFERENCES Dim.Product(ProductKey),
    CONSTRAINT FK_Fact_Sales_Seller FOREIGN KEY (SellerKey) REFERENCES Dim.Seller(SellerKey),
    CONSTRAINT FK_Fact_Sales_Status FOREIGN KEY (StatusKey) REFERENCES Dim.Status(StatusKey),
    CONSTRAINT FK_Fact_Sales_Date FOREIGN KEY (DateKey) REFERENCES Dim.Date(DateKey)
);
GO
```

---

## 5. Consultas SQL Analíticas (`SolucionPreguntasNegocio.sql`)

```sql
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
```
