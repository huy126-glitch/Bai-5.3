
IF DB_ID(N'CafePOSDB') IS NULL
    CREATE DATABASE CafePOSDB;
GO

USE CafePOSDB;
GO

-- 1. Bang BanAn
IF OBJECT_ID(N'dbo.BanAn', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BanAn
    (
        MaBan INT IDENTITY(1,1) PRIMARY KEY,
        TenBan NVARCHAR(50) NOT NULL,
        TrangThai NVARCHAR(20) NOT NULL
            CONSTRAINT DF_BanAn_TrangThai DEFAULT N'Trống'
    );
END
GO

-- 2. Bang DanhMuc
IF OBJECT_ID(N'dbo.DanhMuc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DanhMuc
    (
        MaDanhMuc INT IDENTITY(1,1) PRIMARY KEY,
        TenDanhMuc NVARCHAR(100) NOT NULL
    );
END
GO

-- 3. Bang MonAn
IF OBJECT_ID(N'dbo.MonAn', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MonAn
    (
        MaMon INT IDENTITY(1,1) PRIMARY KEY,
        TenMon NVARCHAR(100) NOT NULL,
        MaDanhMuc INT NOT NULL,
        DonGia DECIMAL(18,2) NOT NULL
            CHECK (DonGia >= 0),
        CONSTRAINT FK_MonAn_DanhMuc
            FOREIGN KEY (MaDanhMuc)
            REFERENCES dbo.DanhMuc(MaDanhMuc)
    );
END
GO

-- 4. Bang HoaDon
IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDon
    (
        MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
        MaBan INT NOT NULL,
        GioVao DATETIME NOT NULL
            CONSTRAINT DF_HoaDon_GioVao DEFAULT GETDATE(),
        GioRa DATETIME NULL,
        TongTien DECIMAL(18,2) NOT NULL DEFAULT 0,
        TrangThai BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_HoaDon_BanAn
            FOREIGN KEY (MaBan) REFERENCES dbo.BanAn(MaBan)
    );
END
GO

-- 5. Bang ChiTietHoaDon
IF OBJECT_ID(N'dbo.ChiTietHoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietHoaDon
    (
        MaHoaDon INT NOT NULL,
        MaMon INT NOT NULL,
        SoLuong INT NOT NULL CHECK (SoLuong > 0),
        DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
        CONSTRAINT PK_ChiTietHoaDon
            PRIMARY KEY (MaHoaDon, MaMon),
        CONSTRAINT FK_CTHD_HoaDon
            FOREIGN KEY (MaHoaDon)
            REFERENCES dbo.HoaDon(MaHoaDon),
        CONSTRAINT FK_CTHD_MonAn
            FOREIGN KEY (MaMon)
            REFERENCES dbo.MonAn(MaMon)
    );
END
GO

-- Them ban mau neu chua co
IF NOT EXISTS (SELECT 1 FROM dbo.BanAn)
BEGIN
    INSERT INTO dbo.BanAn (TenBan, TrangThai)
    VALUES
        (N'Bàn 01', N'Trống'),
        (N'Bàn 02', N'Trống'),
        (N'Bàn 03', N'Trống'),
        (N'Bàn 04', N'Trống'),
        (N'Bàn 05', N'Trống'),
        (N'Bàn 06', N'Trống'),
        (N'Bàn 07', N'Trống'),
        (N'Bàn 08', N'Trống');
END
GO

-- Them danh muc mau
IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc)
BEGIN
    INSERT INTO dbo.DanhMuc (TenDanhMuc)
    VALUES
        (N'Cà phê'),
        (N'Trà sữa'),
        (N'Đồ ăn nhẹ');
END
GO

-- Them mon an mau
IF NOT EXISTS (SELECT 1 FROM dbo.MonAn)
BEGIN
    INSERT INTO dbo.MonAn (TenMon, MaDanhMuc, DonGia)
    SELECT N'Cà phê đen', MaDanhMuc, 20000
    FROM dbo.DanhMuc WHERE TenDanhMuc = N'Cà phê';

    INSERT INTO dbo.MonAn (TenMon, MaDanhMuc, DonGia)
    SELECT N'Cà phê sữa', MaDanhMuc, 25000
    FROM dbo.DanhMuc WHERE TenDanhMuc = N'Cà phê';

    INSERT INTO dbo.MonAn (TenMon, MaDanhMuc, DonGia)
    SELECT N'Trà sữa truyền thống', MaDanhMuc, 35000
    FROM dbo.DanhMuc WHERE TenDanhMuc = N'Trà sữa';

    INSERT INTO dbo.MonAn (TenMon, MaDanhMuc, DonGia)
    SELECT N'Trà sữa matcha', MaDanhMuc, 40000
    FROM dbo.DanhMuc WHERE TenDanhMuc = N'Trà sữa';

    INSERT INTO dbo.MonAn (TenMon, MaDanhMuc, DonGia)
    SELECT N'Bánh ngọt', MaDanhMuc, 30000
    FROM dbo.DanhMuc WHERE TenDanhMuc = N'Đồ ăn nhẹ';

    INSERT INTO dbo.MonAn (TenMon, MaDanhMuc, DonGia)
    SELECT N'Khoai tây chiên', MaDanhMuc, 35000
    FROM dbo.DanhMuc WHERE TenDanhMuc = N'Đồ ăn nhẹ';
END
GO

SELECT * FROM dbo.BanAn;
SELECT * FROM dbo.DanhMuc;
SELECT * FROM dbo.MonAn;
SELECT * FROM dbo.HoaDon;
GO
