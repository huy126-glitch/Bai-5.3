
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CafePOS
{
    public class CafeService
    {
        // Nap du lieu bang disconnected model
        public DataTable GetTables()
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT MaBan, TenBan, TrangThai FROM dbo.BanAn ORDER BY MaBan",
                conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetCategories()
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT MaDanhMuc, TenDanhMuc FROM dbo.DanhMuc ORDER BY TenDanhMuc",
                conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetMenu(int maDanhMuc)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                @"SELECT MaMon, TenMon, MaDanhMuc, DonGia
                  FROM dbo.MonAn
                  WHERE MaDanhMuc = @MaDanhMuc
                  ORDER BY TenMon", conn))
            {
                da.SelectCommand.Parameters.Add(
                    "@MaDanhMuc", SqlDbType.Int).Value = maDanhMuc;

                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetAllMenu()
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                @"SELECT m.MaMon, m.TenMon, m.MaDanhMuc,
                         d.TenDanhMuc, m.DonGia
                  FROM dbo.MonAn m
                  INNER JOIN dbo.DanhMuc d
                    ON m.MaDanhMuc = d.MaDanhMuc
                  ORDER BY m.MaMon", conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Chi tiet hoa don dang mo
        public DataTable GetOrderDetails(int maBan)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                @"SELECT ct.MaMon, m.TenMon,
                         ct.SoLuong, ct.DonGia,
                         ct.SoLuong * ct.DonGia AS ThanhTien
                  FROM dbo.HoaDon hd
                  INNER JOIN dbo.ChiTietHoaDon ct
                    ON hd.MaHoaDon = ct.MaHoaDon
                  INNER JOIN dbo.MonAn m
                    ON ct.MaMon = m.MaMon
                  WHERE hd.MaBan = @MaBan
                    AND hd.TrangThai = 0
                  ORDER BY m.TenMon", conn))
            {
                da.SelectCommand.Parameters.Add(
                    "@MaBan", SqlDbType.Int).Value = maBan;

                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public decimal GetOrderTotal(int maBan)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT COALESCE(SUM(ct.SoLuong * ct.DonGia), 0)
                  FROM dbo.HoaDon hd
                  INNER JOIN dbo.ChiTietHoaDon ct
                    ON hd.MaHoaDon = ct.MaHoaDon
                  WHERE hd.MaBan = @MaBan
                    AND hd.TrangThai = 0", conn))
            {
                cmd.Parameters.Add(
                    "@MaBan", SqlDbType.Int).Value = maBan;

                conn.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        // Them mon vao hoa don dang mo
        public void AddOrderItem(int maBan, int maMon, int soLuong)
        {
            if (soLuong <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0.");

            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();

                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        int maHoaDon;

                        // Kiem tra ban co hoa don chua thanh toan
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT MaHoaDon
                              FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
                              WHERE MaBan = @MaBan AND TrangThai = 0",
                            conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@MaBan", SqlDbType.Int).Value = maBan;

                            object result = cmd.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                using (SqlCommand create = new SqlCommand(
                                    @"INSERT INTO dbo.HoaDon
                                        (MaBan, GioVao, TongTien, TrangThai)
                                      VALUES
                                        (@MaBan, GETDATE(), 0, 0);
                                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                                    conn, tran))
                                {
                                    create.Parameters.Add(
                                        "@MaBan", SqlDbType.Int).Value = maBan;

                                    maHoaDon = Convert.ToInt32(
                                        create.ExecuteScalar());
                                }
                            }
                            else
                            {
                                maHoaDon = Convert.ToInt32(result);
                            }
                        }

                        // Lay gia hien tai tu menu
                        decimal donGia;

                        using (SqlCommand price = new SqlCommand(
                            "SELECT DonGia FROM dbo.MonAn WHERE MaMon = @MaMon",
                            conn, tran))
                        {
                            price.Parameters.Add(
                                "@MaMon", SqlDbType.Int).Value = maMon;

                            object value = price.ExecuteScalar();

                            if (value == null || value == DBNull.Value)
                                throw new Exception("Món ăn không tồn tại.");

                            donGia = Convert.ToDecimal(value);
                        }

                        // Neu mon da co thi cong them so luong
                        using (SqlCommand update = new SqlCommand(
                            @"UPDATE dbo.ChiTietHoaDon
                              SET SoLuong = SoLuong + @SoLuong
                              WHERE MaHoaDon = @MaHoaDon
                                AND MaMon = @MaMon",
                            conn, tran))
                        {
                            update.Parameters.Add(
                                "@SoLuong", SqlDbType.Int).Value = soLuong;
                            update.Parameters.Add(
                                "@MaHoaDon", SqlDbType.Int).Value = maHoaDon;
                            update.Parameters.Add(
                                "@MaMon", SqlDbType.Int).Value = maMon;

                            int affected = update.ExecuteNonQuery();

                            if (affected == 0)
                            {
                                using (SqlCommand insert = new SqlCommand(
                                    @"INSERT INTO dbo.ChiTietHoaDon
                                        (MaHoaDon, MaMon, SoLuong, DonGia)
                                      VALUES
                                        (@MaHoaDon, @MaMon, @SoLuong, @DonGia)",
                                    conn, tran))
                                {
                                    insert.Parameters.Add(
                                        "@MaHoaDon", SqlDbType.Int).Value = maHoaDon;
                                    insert.Parameters.Add(
                                        "@MaMon", SqlDbType.Int).Value = maMon;
                                    insert.Parameters.Add(
                                        "@SoLuong", SqlDbType.Int).Value = soLuong;
                                    insert.Parameters.Add(
                                        "@DonGia", SqlDbType.Decimal).Value = donGia;

                                    insert.ExecuteNonQuery();
                                }
                            }
                        }

                        using (SqlCommand updateTable = new SqlCommand(
                            "UPDATE dbo.BanAn SET TrangThai = N'Có khách' WHERE MaBan = @MaBan",
                            conn, tran))
                        {
                            updateTable.Parameters.Add(
                                "@MaBan", SqlDbType.Int).Value = maBan;
                            updateTable.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        // Thanh toan trong mot transaction
        public decimal PayOrder(int maBan)
        {
            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();

                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        int maHoaDon;

                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT MaHoaDon
                              FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
                              WHERE MaBan = @MaBan AND TrangThai = 0",
                            conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@MaBan", SqlDbType.Int).Value = maBan;

                            object result = cmd.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                                throw new Exception("Bàn chưa có hóa đơn cần thanh toán.");

                            maHoaDon = Convert.ToInt32(result);
                        }

                        decimal tongTien;

                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT COALESCE(SUM(SoLuong * DonGia), 0)
                              FROM dbo.ChiTietHoaDon
                              WHERE MaHoaDon = @MaHoaDon",
                            conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@MaHoaDon", SqlDbType.Int).Value = maHoaDon;

                            tongTien = Convert.ToDecimal(cmd.ExecuteScalar());
                        }

                        if (tongTien <= 0)
                            throw new Exception("Hóa đơn chưa có món ăn.");

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE dbo.HoaDon
                              SET GioRa = GETDATE(),
                                  TongTien = @TongTien,
                                  TrangThai = 1
                              WHERE MaHoaDon = @MaHoaDon
                                AND TrangThai = 0",
                            conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@TongTien", SqlDbType.Decimal).Value = tongTien;
                            cmd.Parameters.Add(
                                "@MaHoaDon", SqlDbType.Int).Value = maHoaDon;

                            if (cmd.ExecuteNonQuery() != 1)
                                throw new Exception("Không thể cập nhật hóa đơn.");
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "UPDATE dbo.BanAn SET TrangThai = N'Trống' WHERE MaBan = @MaBan",
                            conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@MaBan", SqlDbType.Int).Value = maBan;
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return tongTien;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        // Doi ban: chuyen hoa don chua thanh toan sang ban trong
        public void MoveTable(int maBanCu, int maBanMoi)
        {
            if (maBanCu == maBanMoi)
                return;

            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();

                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand check = new SqlCommand(
                            @"SELECT COUNT(*)
                              FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
                              WHERE MaBan = @MaBanMoi AND TrangThai = 0",
                            conn, tran))
                        {
                            check.Parameters.Add(
                                "@MaBanMoi", SqlDbType.Int).Value = maBanMoi;

                            if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                                throw new Exception("Bàn đích đang có khách.");
                        }

                        using (SqlCommand update = new SqlCommand(
                            @"UPDATE dbo.HoaDon
                              SET MaBan = @MaBanMoi
                              WHERE MaBan = @MaBanCu AND TrangThai = 0",
                            conn, tran))
                        {
                            update.Parameters.Add(
                                "@MaBanMoi", SqlDbType.Int).Value = maBanMoi;
                            update.Parameters.Add(
                                "@MaBanCu", SqlDbType.Int).Value = maBanCu;

                            if (update.ExecuteNonQuery() == 0)
                                throw new Exception("Bàn cũ chưa có hóa đơn mở.");
                        }

                        using (SqlCommand update = new SqlCommand(
                            @"UPDATE dbo.BanAn
                              SET TrangThai =
                                  CASE
                                    WHEN MaBan = @MaBanCu THEN N'Trống'
                                    WHEN MaBan = @MaBanMoi THEN N'Có khách'
                                    ELSE TrangThai
                                  END
                              WHERE MaBan IN (@MaBanCu, @MaBanMoi)",
                            conn, tran))
                        {
                            update.Parameters.Add(
                                "@MaBanCu", SqlDbType.Int).Value = maBanCu;
                            update.Parameters.Add(
                                "@MaBanMoi", SqlDbType.Int).Value = maBanMoi;
                            update.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        // Them mon
        public void AddMenuItem(string tenMon, int maDanhMuc, decimal donGia)
        {
            ValidateMenuItem(tenMon, donGia);

            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO dbo.MonAn (TenMon, MaDanhMuc, DonGia)
                  VALUES (@TenMon, @MaDanhMuc, @DonGia)", conn))
            {
                cmd.Parameters.Add(
                    "@TenMon", SqlDbType.NVarChar, 100).Value = tenMon.Trim();
                cmd.Parameters.Add(
                    "@MaDanhMuc", SqlDbType.Int).Value = maDanhMuc;

                SqlParameter price = cmd.Parameters.Add(
                    "@DonGia", SqlDbType.Decimal);
                price.Precision = 18;
                price.Scale = 2;
                price.Value = donGia;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Sua mon
        public void UpdateMenuItem(int maMon, string tenMon,
            int maDanhMuc, decimal donGia)
        {
            ValidateMenuItem(tenMon, donGia);

            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"UPDATE dbo.MonAn
                  SET TenMon = @TenMon,
                      MaDanhMuc = @MaDanhMuc,
                      DonGia = @DonGia
                  WHERE MaMon = @MaMon", conn))
            {
                cmd.Parameters.Add(
                    "@MaMon", SqlDbType.Int).Value = maMon;
                cmd.Parameters.Add(
                    "@TenMon", SqlDbType.NVarChar, 100).Value = tenMon.Trim();
                cmd.Parameters.Add(
                    "@MaDanhMuc", SqlDbType.Int).Value = maDanhMuc;

                SqlParameter price = cmd.Parameters.Add(
                    "@DonGia", SqlDbType.Decimal);
                price.Precision = 18;
                price.Scale = 2;
                price.Value = donGia;

                conn.Open();

                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Không tìm thấy món ăn cần sửa.");
            }
        }

        // Xoa mon
        public void DeleteMenuItem(int maMon)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                "DELETE FROM dbo.MonAn WHERE MaMon = @MaMon", conn))
            {
                cmd.Parameters.Add(
                    "@MaMon", SqlDbType.Int).Value = maMon;

                conn.Open();

                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Không tìm thấy món ăn cần xóa.");
            }
        }

        private void ValidateMenuItem(string tenMon, decimal donGia)
        {
            if (string.IsNullOrWhiteSpace(tenMon))
                throw new ArgumentException("Tên món không được để trống.");

            if (donGia < 0)
                throw new ArgumentException("Đơn giá không được âm.");
        }
    }
}
