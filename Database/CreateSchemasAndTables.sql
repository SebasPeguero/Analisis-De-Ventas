-- Script de creación de estructuras analíticas y de staging
-- Base de datos: SistemaAnalisisVentas

USE SistemaAnalisisVentas;
GO

-- ==========================================
-- 1. CREACIÓN DE SCHEMAS
-- ==========================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Stg')
BEGIN
    EXEC('CREATE SCHEMA Stg;');
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Dim')
BEGIN
    EXEC('CREATE SCHEMA Dim;');
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Fact')
BEGIN
    EXEC('CREATE SCHEMA [Fact];');
END
GO

-- ==========================================
-- 2. ELIMINACIÓN DE TABLAS EXISTENTES (Si aplica)
-- ==========================================
-- Eliminar en orden inverso a dependencias de FK
IF OBJECT_ID('Fact.Sales', 'U') IS NOT NULL DROP TABLE [Fact].Sales;
IF OBJECT_ID('Dim.Customer', 'U') IS NOT NULL DROP TABLE Dim.Customer;
IF OBJECT_ID('Dim.Product', 'U') IS NOT NULL DROP TABLE Dim.Product;
IF OBJECT_ID('Dim.Status', 'U') IS NOT NULL DROP TABLE Dim.Status;
IF OBJECT_ID('Dim.Date', 'U') IS NOT NULL DROP TABLE Dim.Date;

IF OBJECT_ID('Stg.Customers', 'U') IS NOT NULL DROP TABLE Stg.Customers;
IF OBJECT_ID('Stg.Products', 'U') IS NOT NULL DROP TABLE Stg.Products;
IF OBJECT_ID('Stg.Orders', 'U') IS NOT NULL DROP TABLE Stg.Orders;
IF OBJECT_ID('Stg.OrderDetails', 'U') IS NOT NULL DROP TABLE Stg.OrderDetails;
GO

-- ==========================================
-- 3. TABLAS DE STAGING (Esquema Stg)
-- ==========================================
CREATE TABLE Stg.Customers (
    CustomerID   INT,
    FirstName    NVARCHAR(100),
    LastName     NVARCHAR(100),
    Email        NVARCHAR(150),
    Phone        NVARCHAR(50),
    City         NVARCHAR(100),
    Country      NVARCHAR(100),
    BatchID      INT,
    LoadedAt     DATETIME2 DEFAULT SYSDATETIME()
);

CREATE TABLE Stg.Products (
    ProductID    INT,
    ProductName  NVARCHAR(150),
    Category     NVARCHAR(100),
    Price        DECIMAL(18,2),
    Stock        INT,
    BatchID      INT,
    LoadedAt     DATETIME2 DEFAULT SYSDATETIME()
);

CREATE TABLE Stg.Orders (
    OrderID      INT,
    CustomerID   INT,
    OrderDate    DATE,
    Status       NVARCHAR(50),
    BatchID      INT,
    LoadedAt     DATETIME2 DEFAULT SYSDATETIME()
);

CREATE TABLE Stg.OrderDetails (
    OrderID      INT,
    ProductID    INT,
    Quantity     INT,
    TotalPrice   DECIMAL(18,2),
    BatchID      INT,
    LoadedAt     DATETIME2 DEFAULT SYSDATETIME()
);
GO

-- ==========================================
-- 4. TABLAS DE DIMENSIONES (Esquema Dim)
-- ==========================================
CREATE TABLE Dim.Customer (
    CustomerKey  INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID   INT NOT NULL,        -- Referencia natural a Master.Customers
    FullName     NVARCHAR(200),
    Email        NVARCHAR(150),
    CityName     NVARCHAR(100),
    CountryName  NVARCHAR(100)
);

CREATE TABLE Dim.Product (
    ProductKey   INT IDENTITY(1,1) PRIMARY KEY,
    ProductID    INT NOT NULL,        -- Referencia natural a Master.Products
    ProductName  NVARCHAR(150),
    CategoryName NVARCHAR(100)
);

CREATE TABLE Dim.Status (
    StatusKey    INT IDENTITY(1,1) PRIMARY KEY,
    StatusID     INT NOT NULL,        -- Referencia natural a Master.OrderStatus
    StatusName   NVARCHAR(50)
);

CREATE TABLE Dim.Date (
    DateKey      INT PRIMARY KEY,     -- Formato YYYYMMDD
    FullDate     DATE NOT NULL,
    [Year]       INT NOT NULL,
    [Month]      INT NOT NULL,
    MonthName    NVARCHAR(20) NOT NULL,
    [Quarter]    INT NOT NULL
);
GO

-- ==========================================
-- 5. TABLA DE HECHOS (Esquema Fact)
-- ==========================================
CREATE TABLE [Fact].Sales (
    SalesKey       BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderID        INT NOT NULL,       -- Dimensión degenerada
    OrderDetailID  INT NOT NULL,       -- Grano: detalle de orden
    CustomerKey    INT NOT NULL REFERENCES Dim.Customer(CustomerKey),
    ProductKey     INT NOT NULL REFERENCES Dim.Product(ProductKey),
    StatusKey      INT NOT NULL REFERENCES Dim.Status(StatusKey),
    DateKey        INT NOT NULL REFERENCES Dim.Date(DateKey),
    Quantity       INT NOT NULL,
    UnitPrice      DECIMAL(18,2) NOT NULL,
    Total          DECIMAL(18,2) NOT NULL
);
GO

PRINT 'Esquemas y tablas analíticas creadas exitosamente.';
GO
