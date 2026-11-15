-- Database Initialization Script for B2B Wholesale Order Management System
-- Group 02 - Software Engineering (FIT - TDTU)
-- Members: Trịnh Đức Long (524K0021), Trần Hoàng Anh (524K0015), Võ Thị Mỹ Duyên (523V0016)

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'B2BWholesaleDB')
BEGIN
    CREATE DATABASE B2BWholesaleDB;
END
GO

USE B2BWholesaleDB;
GO

-- Drop existing tables if they exist
IF OBJECT_ID('dbo.OrderDetail', 'U') IS NOT NULL DROP TABLE dbo.OrderDetail;
IF OBJECT_ID('dbo.[Order]', 'U') IS NOT NULL DROP TABLE dbo.[Order];
IF OBJECT_ID('dbo.Item', 'U') IS NOT NULL DROP TABLE dbo.Item;
IF OBJECT_ID('dbo.Agent', 'U') IS NOT NULL DROP TABLE dbo.Agent;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- Users table: login credentials, roles, and lockout
CREATE TABLE dbo.Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(256) NOT NULL, -- SHA-256 hashed or password
    Lock BIT NOT NULL DEFAULT 0,     -- 0: Active, 1: Locked
    Role NVARCHAR(20) NOT NULL CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Agent'))
);
GO

-- Agent table: wholesale customer profiles
CREATE TABLE dbo.Agent (
    AgentID INT IDENTITY(1,1) PRIMARY KEY,
    AgentName NVARCHAR(100) NOT NULL,
    Address NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20) NOT NULL
);
GO

-- Item table: catalog products and stock quantities
CREATE TABLE dbo.Item (
    ItemID INT IDENTITY(1,1) PRIMARY KEY,
    ItemName NVARCHAR(150) NOT NULL,
    Size NVARCHAR(50) NOT NULL,
    Price DECIMAL(18,2) NOT NULL CONSTRAINT CK_Item_Price CHECK (Price >= 0),
    StockQuantity INT NOT NULL CONSTRAINT CK_Item_StockQuantity CHECK (StockQuantity >= 0)
);
GO

-- Order table: order headers
CREATE TABLE dbo.[Order] (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    OrderDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    AgentID INT NOT NULL,
    OrderStatus NVARCHAR(30) NOT NULL DEFAULT 'Pending' 
        CONSTRAINT CK_Order_Status CHECK (OrderStatus IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled')),
    CONSTRAINT FK_Order_Agent FOREIGN KEY (AgentID) REFERENCES dbo.Agent(AgentID)
        ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- OrderDetail table: itemized order line items
CREATE TABLE dbo.OrderDetail (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ItemID INT NOT NULL,
    Quantity INT NOT NULL CONSTRAINT CK_OrderDetail_Quantity CHECK (Quantity > 0),
    UnitAmount DECIMAL(18,2) NOT NULL CONSTRAINT CK_OrderDetail_UnitAmount CHECK (UnitAmount >= 0),
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderID) REFERENCES dbo.[Order](OrderID)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_OrderDetail_Item FOREIGN KEY (ItemID) REFERENCES dbo.Item(ItemID)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- Seed sample data: 20 rows per table

-- Seed Users: Admin accounts (Admin@123) and Agent accounts (Agent@123)
INSERT INTO dbo.Users (UserName, Email, Password, Lock, Role) VALUES
('admin_main', 'admin@b2bwholesale.vn', 'Admin@123', 0, 'Admin'),
('warehouse_lead', 'wh_lead@b2bwholesale.vn', 'Admin@123', 0, 'Admin'),
('inventory_admin', 'inventory@b2bwholesale.vn', 'Admin@123', 0, 'Admin'),
('ops_manager', 'ops@b2bwholesale.vn', 'Admin@123', 0, 'Admin'),
('locked_admin', 'suspended@b2bwholesale.vn', 'Admin@123', 1, 'Admin'),
('agent_hanoi01', 'agent.hn01@redriver.com', 'Agent@123', 0, 'Agent'),
('agent_hcm01', 'orders@saigontrading.vn', 'Agent@123', 0, 'Agent'),
('agent_danang01', 'contact@danangsupply.com', 'Agent@123', 0, 'Agent'),
('agent_cantho01', 'sales@mekongdistrib.vn', 'Agent@123', 0, 'Agent'),
('agent_haiphong01', 'order@haiphongmart.com', 'Agent@123', 0, 'Agent'),
('agent_nhatrang01', 'procure@nhatrangretail.com', 'Agent@123', 0, 'Agent'),
('agent_vungtau01', 'wh@vungtauco.vn', 'Agent@123', 0, 'Agent'),
('agent_hue01', 'hue.supply@centralvn.vn', 'Agent@123', 0, 'Agent'),
('agent_binhduong01', 'bd.orders@industrading.com', 'Agent@123', 0, 'Agent'),
('agent_dongnai01', 'purchasing@dongnaisp.vn', 'Agent@123', 0, 'Agent'),
('agent_quangninh01', 'qn.b2b@halongtrade.vn', 'Agent@123', 0, 'Agent'),
('agent_dalat01', 'dalat.fresh@highland.vn', 'Agent@123', 0, 'Agent'),
('agent_thanhhoa01', 'order@thanhhoadist.vn', 'Agent@123', 0, 'Agent'),
('agent_bacninh01', 'bn.logistics@bacninhcorp.vn', 'Agent@123', 0, 'Agent'),
('agent_locked01', 'inactive@oldagent.com', 'Agent@123', 1, 'Agent');
GO

-- Seed Agent records
INSERT INTO dbo.Agent (AgentName, Address, Phone) VALUES
(N'Red River Trading JSC', N'128 Pho Hue, Hai Ba Trung, Ha Noi', '024-3822-1101'),
(N'Saigon Wholesale & Distribution', N'45 Nguyen Hue, District 1, Ho Chi Minh City', '028-3914-2202'),
(N'Da Nang Coastal Supplies Co.', N'76 Bach Dang, Hai Chau, Da Nang', '0236-384-3303'),
(N'Mekong Delta Goods Distribution', N'120 30/4 Street, Ninh Kieu, Can Tho', '0292-383-4404'),
(N'Hai Phong Port Trading Hub', N'88 Le Hong Phong, Ngo Quyen, Hai Phong', '0225-385-5505'),
(N'Nha Trang Ocean Retailers', N'15 Tran Phu, Loc Tho, Nha Trang', '0258-382-6606'),
(N'Vung Tau Oil & Industrial Supplies', N'204 Le Loi, Ward 4, Vung Tau', '0254-383-7707'),
(N'Central Heritage Traders Hue', N'34 Hung Vuong, Phu Hoi, Hue', '0234-382-8808'),
(N'Binh Duong Logistics & Distribution', N'15 VSIP Boulevard, Thuan An, Binh Duong', '0274-376-9909'),
(N'Dong Nai Retail Chain Alliance', N'50 Dong Khoi, Bien Hoa, Dong Nai', '0251-382-1010'),
(N'Ha Long Gulf Commercial Network', N'102 Ha Long Road, Bai Chay, Quang Ninh', '0203-384-1111'),
(N'Dalat Highland Agro Distributors', N'22 Phan Dinh Phung, Ward 2, Da Lat', '0263-382-1212'),
(N'Thanh Hoa Regional Wholesalers', N'45 Le Loi, Lam Son, Thanh Hoa', '0237-385-1313'),
(N'Bac Ninh Tech & Industrial Depot', N'68 Tran Hung Dao, Tien An, Bac Ninh', '0222-387-1414'),
(N'Phu Quoc Island General Provisions', N'90 Tran Hung Dao, Duong Dong, Phu Quoc', '0297-384-1515'),
(N'Quy Nhon Port Merchant Corp', N'12 Le Duan, Ly Thuong Kiet, Quy Nhon', '0256-382-1616'),
(N'Thai Nguyen Northern Central Store', N'33 Luong Ngoc Quyen, Hoang Van Thu, Thai Nguyen', '0208-385-1717'),
(N'An Giang Borderline Wholesalers', N'55 Nguyen Trai, Long Xuyen, An Giang', '0296-384-1818'),
(N'Nam Dinh Textile & Hardware Depot', N'70 Tran Hung Dao, Ba Trieu, Nam Dinh', '0228-384-1919'),
(N'Vinh City Central Warehouse', N'89 Le Duan, Trung Do, Vinh, Nghe An', '0238-383-2020');
GO

-- Seed Item records
INSERT INTO dbo.Item (ItemName, Size, Price, StockQuantity) VALUES
(N'A4 Multipurpose Copy Paper 80gsm (Box of 5 Reams)', N'Box (5 Reams)', 245000, 450),
(N'Premium Ballpoint Pen 0.7mm Blue (Box of 50)', N'Box (50 pcs)', 150000, 1200),
(N'Heavy-Duty Packaging Tape 5cm x 100m (Pack of 6)', N'Pack (6 rolls)', 115000, 850),
(N'Industrial Bubble Wrap Roll 0.5m x 100m', N'Roll', 280000, 320),
(N'Corrugated Shipping Box 30x20x15cm (Bundle of 20)', N'Bundle (20 pcs)', 160000, 600),
(N'Corrugated Shipping Box 40x30x25cm (Bundle of 20)', N'Bundle (20 pcs)', 240000, 520),
(N'Heavy-Duty Stretch Film Wrap 50cm x 3kg', N'Roll', 185000, 410),
(N'LED Industrial High-Bay Light 100W', N'Carton (4 pcs)', 1250000, 85),
(N'Cat6 UTP Network Cable 305m Roll', N'Roll (305m)', 1650000, 95),
(N'Wireless Barcode Scanner Handheld USB/2.4G', N'Unit', 890000, 140),
(N'Direct Thermal Shipping Labels 100x150mm (Roll of 350)', N'Roll (350 labels)', 75000, 1500),
(N'Thermal Receipt Paper 80x80mm (Box of 50)', N'Box (50 rolls)', 620000, 230),
(N'3-Ply Protective Facemask (Case of 50 Boxes)', N'Case (2500 pcs)', 1200000, 180),
(N'Nitrile Examination Gloves Powder-Free (Box of 100)', N'Box (100 pcs)', 110000, 900),
(N'Multi-Surface Industrial Floor Cleaner 5L', N'Canister 5L', 195000, 340),
(N'Antibacterial Hand Sanitizer Gel 5L', N'Canister 5L', 310000, 150),
(N'Safety Steel-Toe Work Boots (Size 42)', N'Pair', 450000, 75),
(N'Safety High-Visibility Reflective Vest', N'Pack (10 pcs)', 220000, 480),
(N'Heavy Duty Hand Pallet Truck 2.5 Tons', N'Unit', 4850000, 15),
(N'Digital Platform Weighing Scale 300kg', N'Unit', 1750000, 28);
GO

-- Seed Order records
INSERT INTO dbo.[Order] (OrderDate, AgentID, OrderStatus) VALUES
('2026-10-01 08:30:00', 1, 'Delivered'),
('2026-10-03 09:15:00', 2, 'Delivered'),
('2026-10-05 14:00:00', 3, 'Delivered'),
('2026-10-08 10:45:00', 4, 'Delivered'),
('2026-10-12 11:20:00', 5, 'Shipped'),
('2026-10-15 16:30:00', 6, 'Shipped'),
('2026-10-18 09:00:00', 7, 'Shipped'),
('2026-10-22 13:10:00', 8, 'Processing'),
('2026-10-25 15:50:00', 9, 'Processing'),
('2026-10-28 10:05:00', 10, 'Processing'),
('2026-11-01 08:40:00', 1, 'Processing'),
('2026-11-03 11:30:00', 2, 'Processing'),
('2026-11-05 14:15:00', 11, 'Pending'),
('2026-11-07 09:20:00', 12, 'Pending'),
('2026-11-09 16:00:00', 13, 'Pending'),
('2026-11-10 10:30:00', 14, 'Pending'),
('2026-11-11 13:45:00', 15, 'Pending'),
('2026-11-12 15:10:00', 3, 'Pending'),
('2026-11-13 11:00:00', 4, 'Pending'),
('2026-11-14 09:30:00', 5, 'Cancelled');
GO

-- Seed OrderDetail recordsds
INSERT INTO dbo.OrderDetail (OrderID, ItemID, Quantity, UnitAmount) VALUES
(1, 1, 10, 245000),
(1, 2, 5, 150000),
(2, 3, 20, 115000),
(2, 5, 15, 160000),
(3, 4, 8, 280000),
(3, 7, 12, 185000),
(4, 9, 3, 1650000),
(4, 10, 4, 890000),
(5, 11, 30, 75000),
(5, 12, 10, 620000),
(6, 13, 5, 1200000),
(6, 14, 25, 110000),
(7, 15, 15, 195000),
(7, 16, 10, 310000),
(8, 17, 10, 450000),
(8, 18, 15, 220000),
(9, 19, 1, 4850000),
(10, 20, 2, 1750000),
(11, 1, 15, 245000),
(11, 3, 10, 115000),
(12, 5, 25, 160000),
(12, 6, 20, 240000),
(13, 2, 10, 150000),
(14, 11, 50, 75000),
(15, 8, 4, 1250000);
GO

PRINT 'B2B Wholesale Database initialized successfully with sample data.';
GO
