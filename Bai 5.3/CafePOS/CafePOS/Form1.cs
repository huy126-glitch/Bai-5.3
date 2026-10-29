
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CafePOS
{
    public partial class Form1 : Form
    {
        private readonly CafeService service = new CafeService();

        private TabControl tabs;
        private FlowLayoutPanel pnlTables;
        private DataGridView dgvOrder;
        private ComboBox cboCategory;
        private ComboBox cboMenu;
        private NumericUpDown nudQuantity;
        private ComboBox cboMoveTable;
        private Label lblSelectedTable;
        private Label lblTotal;

        private DataGridView dgvMenu;
        private TextBox txtTenMon;
        private ComboBox cboMenuCategory;
        private NumericUpDown nudPrice;
        private int selectedTableId = -1;
        private int selectedMenuId = -1;

        public Form1()
        {
            InitializeComponent();
            Controls.Clear();

            Text = "Cafe POS - Quản lý quán Cafe";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 760);
            MinimumSize = new Size(1000, 650);
            Font = new Font("Segoe UI", 10);

            BuildInterface();
            Load += Form1_Load;
        }

        private void BuildInterface()
        {
            tabs = new TabControl { Dock = DockStyle.Fill };

            TabPage tabPOS = new TabPage("Bán hàng");
            TabPage tabMenu = new TabPage("Quản lý thực đơn");

            tabs.TabPages.Add(tabPOS);
            tabs.TabPages.Add(tabMenu);
            Controls.Add(tabs);

            BuildPOSTab(tabPOS);
            BuildMenuTab(tabMenu);
        }

        private void BuildPOSTab(TabPage page)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(8)
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 38));
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 62));

            page.Controls.Add(layout);

            // Cot trai: so do ban
            GroupBox grpTables = new GroupBox
            {
                Text = "Sơ đồ bàn",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            pnlTables = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(5)
            };

            grpTables.Controls.Add(pnlTables);
            layout.Controls.Add(grpTables, 0, 0);

            // Cot phai: goi mon va hoa don
            TableLayoutPanel right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(5)
            };

            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 95));

            layout.Controls.Add(right, 1, 0);

            lblSelectedTable = new Label
            {
                Text = "Chưa chọn bàn",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            right.Controls.Add(lblSelectedTable, 0, 0);

            // Khu vuc chon mon
            GroupBox grpOrder = new GroupBox
            {
                Text = "Gọi món",
                Dock = DockStyle.Fill
            };

            TableLayoutPanel orderInputs = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(5)
            };

            orderInputs.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 30));
            orderInputs.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 30));
            orderInputs.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 18));
            orderInputs.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 22));

            cboCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cboMenu = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            nudQuantity = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 1,
                Maximum = 999,
                Value = 1
            };

            Button btnAdd = new Button
            {
                Text = "Thêm món",
                Dock = DockStyle.Fill
            };
            btnAdd.Click += BtnAdd_Click;

            orderInputs.Controls.Add(
                new Label { Text = "Danh mục", Dock = DockStyle.Fill }, 0, 0);
            orderInputs.Controls.Add(
                new Label { Text = "Món ăn", Dock = DockStyle.Fill }, 1, 0);
            orderInputs.Controls.Add(
                new Label { Text = "Số lượng", Dock = DockStyle.Fill }, 2, 0);

            orderInputs.Controls.Add(cboCategory, 0, 1);
            orderInputs.Controls.Add(cboMenu, 1, 1);
            orderInputs.Controls.Add(nudQuantity, 2, 1);
            orderInputs.Controls.Add(btnAdd, 3, 1);

            grpOrder.Controls.Add(orderInputs);
            right.Controls.Add(grpOrder, 0, 1);

            // Bang hoa don
            dgvOrder = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            right.Controls.Add(dgvOrder, 0, 2);

            lblTotal = new Label
            {
                Text = "Tổng tiền: 0 VNĐ",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            right.Controls.Add(lblTotal, 0, 3);

            // Doi ban va thanh toan
            TableLayoutPanel bottom = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(3)
            };

            bottom.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 36));
            bottom.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 32));
            bottom.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 32));

            bottom.Controls.Add(
                new Label
                {
                    Text = "Chuyển sang bàn:",
                    Dock = DockStyle.Fill
                }, 0, 0);

            cboMoveTable = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            bottom.Controls.Add(cboMoveTable, 0, 1);

            Button btnMove = new Button
            {
                Text = "Đổi bàn",
                Dock = DockStyle.Fill
            };
            btnMove.Click += BtnMove_Click;
            bottom.Controls.Add(btnMove, 1, 1);

            Button btnPay = new Button
            {
                Text = "THANH TOÁN",
                Dock = DockStyle.Fill,
                BackColor = Color.LightGreen
            };
            btnPay.Click += BtnPay_Click;
            bottom.Controls.Add(btnPay, 2, 1);

            right.Controls.Add(bottom, 0, 4);

            cboCategory.SelectedIndexChanged +=
                CboCategory_SelectedIndexChanged;
        }

        private void BuildMenuTab(TabPage page)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 65));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 35));

            page.Controls.Add(layout);

            dgvMenu = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            dgvMenu.CellClick += DgvMenu_CellClick;
            layout.Controls.Add(dgvMenu, 0, 0);

            GroupBox grpEdit = new GroupBox
            {
                Text = "Thông tin món ăn",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            TableLayoutPanel edit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };

            for (int i = 0; i < 4; i++)
                edit.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 25));

            edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            edit.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            grpEdit.Controls.Add(edit);
            layout.Controls.Add(grpEdit, 0, 1);

            edit.Controls.Add(new Label
            {
                Text = "Tên món:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            txtTenMon = new TextBox { Dock = DockStyle.Fill };
            edit.Controls.Add(txtTenMon, 1, 0);

            edit.Controls.Add(new Label
            {
                Text = "Danh mục:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 2, 0);

            cboMenuCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            edit.Controls.Add(cboMenuCategory, 3, 0);

            edit.Controls.Add(new Label
            {
                Text = "Đơn giá:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            nudPrice = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 1000000000,
                DecimalPlaces = 2,
                ThousandsSeparator = true
            };
            edit.Controls.Add(nudPrice, 1, 1);

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true
            };

            Button btnNew = new Button { Text = "Thêm món" };
            Button btnUpdate = new Button { Text = "Sửa món" };
            Button btnDelete = new Button { Text = "Xóa món" };
            Button btnClear = new Button { Text = "Làm mới" };

            btnNew.Click += BtnNewMenu_Click;
            btnUpdate.Click += BtnUpdateMenu_Click;
            btnDelete.Click += BtnDeleteMenu_Click;
            btnClear.Click += (s, e) => ClearMenuInputs();

            buttons.Controls.AddRange(new Control[]
            {
                btnNew, btnUpdate, btnDelete, btnClear
            });

            edit.Controls.Add(buttons, 0, 2);
            edit.SetColumnSpan(buttons, 4);
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                LoadCategories();
                LoadTables();
                LoadAllMenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể khởi tạo dữ liệu.\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadCategories()
        {
            DataTable categories = service.GetCategories();

            cboCategory.DataSource = categories.Copy();
            cboCategory.DisplayMember = "TenDanhMuc";
            cboCategory.ValueMember = "MaDanhMuc";

            cboMenuCategory.DataSource = categories.Copy();
            cboMenuCategory.DisplayMember = "TenDanhMuc";
            cboMenuCategory.ValueMember = "MaDanhMuc";

            if (categories.Rows.Count > 0)
            {
                cboCategory.SelectedIndex = 0;
                cboMenuCategory.SelectedIndex = 0;
            }
        }

        private void LoadTables()
        {
            DataTable tables = service.GetTables();

            pnlTables.Controls.Clear();

            foreach (DataRow row in tables.Rows)
            {
                int id = Convert.ToInt32(row["MaBan"]);
                string name = row["TenBan"].ToString() ?? "";
                string status = row["TrangThai"].ToString() ?? "Trống";

                Button btn = new Button
                {
                    Text = name + "\n" + status,
                    Tag = id,
                    Width = 115,
                    Height = 80,
                    Margin = new Padding(6),
                    BackColor = status == "Có khách"
                        ? Color.IndianRed
                        : Color.LightGreen,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat
                };

                btn.Click += TableButton_Click;
                pnlTables.Controls.Add(btn);
            }

            cboMoveTable.DataSource = tables.Copy();
            cboMoveTable.DisplayMember = "TenBan";
            cboMoveTable.ValueMember = "MaBan";

            if (cboMoveTable.Items.Count > 0)
                cboMoveTable.SelectedIndex = -1;
        }

        private void TableButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn)
                return;

            selectedTableId = Convert.ToInt32(btn.Tag);

            lblSelectedTable.Text =
                "Đang chọn: " + btn.Text.Split('\n')[0];

            LoadOrder();
        }

        private void LoadOrder()
        {
            if (selectedTableId < 0)
                return;

            // Nap hoa don tam bang DataTable disconnected
            DataTable order = service.GetOrderDetails(selectedTableId);
            dgvOrder.DataSource = order;

            if (dgvOrder.Columns.Contains("MaMon"))
                dgvOrder.Columns["MaMon"].HeaderText = "Mã món";

            if (dgvOrder.Columns.Contains("TenMon"))
                dgvOrder.Columns["TenMon"].HeaderText = "Tên món";

            if (dgvOrder.Columns.Contains("SoLuong"))
                dgvOrder.Columns["SoLuong"].HeaderText = "Số lượng";

            if (dgvOrder.Columns.Contains("DonGia"))
                dgvOrder.Columns["DonGia"].HeaderText = "Đơn giá";

            if (dgvOrder.Columns.Contains("ThanhTien"))
                dgvOrder.Columns["ThanhTien"].HeaderText = "Thành tiền";

            if (dgvOrder.Columns.Contains("DonGia"))
                dgvOrder.Columns["DonGia"].DefaultCellStyle.Format = "N0";

            if (dgvOrder.Columns.Contains("ThanhTien"))
                dgvOrder.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";

            lblTotal.Text =
                "Tổng tiền: " +
                service.GetOrderTotal(selectedTableId).ToString("N0") +
                " VNĐ";
        }

        private void CboCategory_SelectedIndexChanged(
            object? sender, EventArgs e)
        {
            if (cboCategory.SelectedValue == null ||
                cboCategory.SelectedValue is DataRowView)
                return;

            try
            {
                int categoryId = Convert.ToInt32(cboCategory.SelectedValue);

                DataTable menu = service.GetMenu(categoryId);

                cboMenu.DataSource = menu;
                cboMenu.DisplayMember = "TenMon";
                cboMenu.ValueMember = "MaMon";

                if (menu.Rows.Count > 0)
                    cboMenu.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi tải thực đơn");
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (selectedTableId < 0)
            {
                MessageBox.Show("Vui lòng chọn bàn trước.");
                return;
            }

            if (cboMenu.SelectedValue == null ||
                cboMenu.SelectedValue is DataRowView)
            {
                MessageBox.Show("Vui lòng chọn món ăn.");
                return;
            }

            try
            {
                int maMon = Convert.ToInt32(cboMenu.SelectedValue);
                int soLuong = Convert.ToInt32(nudQuantity.Value);

                service.AddOrderItem(selectedTableId, maMon, soLuong);

                LoadTables();
                LoadOrder();

                MessageBox.Show(
                    "Đã thêm món vào hóa đơn.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message, "Lỗi gọi món",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnPay_Click(object? sender, EventArgs e)
        {
            if (selectedTableId < 0)
            {
                MessageBox.Show("Vui lòng chọn bàn cần thanh toán.");
                return;
            }

            if (dgvOrder.Rows.Count == 0)
            {
                MessageBox.Show("Bàn chưa có món để thanh toán.");
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Xác nhận thanh toán hóa đơn bàn này?",
                "Thanh toán",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                decimal total = service.PayOrder(selectedTableId);

                MessageBox.Show(
                    "Thanh toán thành công!\nTổng tiền: " +
                    total.ToString("N0") + " VNĐ",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                selectedTableId = -1;
                lblSelectedTable.Text = "Chưa chọn bàn";
                dgvOrder.DataSource = null;
                lblTotal.Text = "Tổng tiền: 0 VNĐ";

                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message, "Lỗi thanh toán",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnMove_Click(object? sender, EventArgs e)
        {
            if (selectedTableId < 0)
            {
                MessageBox.Show("Vui lòng chọn bàn đang có khách.");
                return;
            }

            if (cboMoveTable.SelectedValue == null ||
                cboMoveTable.SelectedValue is DataRowView)
            {
                MessageBox.Show("Vui lòng chọn bàn đích.");
                return;
            }

            int destination = Convert.ToInt32(cboMoveTable.SelectedValue);

            if (destination == selectedTableId)
            {
                MessageBox.Show("Bàn đích phải khác bàn hiện tại.");
                return;
            }

            try
            {
                service.MoveTable(selectedTableId, destination);

                selectedTableId = destination;
                LoadTables();
                LoadOrder();

                MessageBox.Show("Đổi bàn thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message, "Lỗi đổi bàn",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadAllMenu()
        {
            dgvMenu.DataSource = service.GetAllMenu();

            if (dgvMenu.Columns.Contains("MaMon"))
                dgvMenu.Columns["MaMon"].HeaderText = "Mã món";

            if (dgvMenu.Columns.Contains("TenMon"))
                dgvMenu.Columns["TenMon"].HeaderText = "Tên món";

            if (dgvMenu.Columns.Contains("TenDanhMuc"))
                dgvMenu.Columns["TenDanhMuc"].HeaderText = "Danh mục";

            if (dgvMenu.Columns.Contains("DonGia"))
            {
                dgvMenu.Columns["DonGia"].HeaderText = "Đơn giá";
                dgvMenu.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            }

            if (dgvMenu.Columns.Contains("MaDanhMuc"))
                dgvMenu.Columns["MaDanhMuc"].Visible = false;
        }

        private void DgvMenu_CellClick(
            object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dgvMenu.Rows[e.RowIndex];

            selectedMenuId = Convert.ToInt32(row.Cells["MaMon"].Value);
            txtTenMon.Text = row.Cells["TenMon"].Value?.ToString() ?? "";
            nudPrice.Value = Convert.ToDecimal(row.Cells["DonGia"].Value);

            cboMenuCategory.SelectedValue =
                Convert.ToInt32(row.Cells["MaDanhMuc"].Value);
        }

        private void BtnNewMenu_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateMenuInputs(out string ten, out int category,
                    out decimal price))
                    return;

                service.AddMenuItem(ten, category, price);

                LoadAllMenu();
                ReloadPOSMenu();
                ClearMenuInputs();

                MessageBox.Show("Thêm món thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message, "Lỗi thêm món",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnUpdateMenu_Click(object? sender, EventArgs e)
        {
            if (selectedMenuId < 0)
            {
                MessageBox.Show("Vui lòng chọn món cần sửa trong bảng.");
                return;
            }

            try
            {
                if (!ValidateMenuInputs(out string ten, out int category,
                    out decimal price))
                    return;

                service.UpdateMenuItem(
                    selectedMenuId, ten, category, price);

                LoadAllMenu();
                ReloadPOSMenu();
                ClearMenuInputs();

                MessageBox.Show("Cập nhật món thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message, "Lỗi sửa món",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteMenu_Click(object? sender, EventArgs e)
        {
            if (selectedMenuId < 0)
            {
                MessageBox.Show("Vui lòng chọn món cần xóa.");
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Bạn có chắc muốn xóa món này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                service.DeleteMenuItem(selectedMenuId);

                LoadAllMenu();
                ReloadPOSMenu();
                ClearMenuInputs();

                MessageBox.Show("Xóa món thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể xóa món. Món có thể đã nằm trong hóa đơn.\n\n" +
                    ex.Message,
                    "Lỗi xóa món",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool ValidateMenuInputs(
            out string ten, out int category, out decimal price)
        {
            ten = txtTenMon.Text.Trim();
            category = 0;
            price = nudPrice.Value;

            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Tên món không được để trống.");
                txtTenMon.Focus();
                return false;
            }

            if (cboMenuCategory.SelectedValue == null ||
                cboMenuCategory.SelectedValue is DataRowView)
            {
                MessageBox.Show("Vui lòng chọn danh mục.");
                return false;
            }

            category = Convert.ToInt32(cboMenuCategory.SelectedValue);

            if (price < 0)
            {
                MessageBox.Show("Đơn giá không được âm.");
                nudPrice.Focus();
                return false;
            }

            return true;
        }

        private void ClearMenuInputs()
        {
            selectedMenuId = -1;
            txtTenMon.Clear();
            nudPrice.Value = 0;

            if (cboMenuCategory.Items.Count > 0)
                cboMenuCategory.SelectedIndex = 0;

            dgvMenu.ClearSelection();
        }

        private void ReloadPOSMenu()
        {
            LoadCategories();
            LoadTables();

            if (selectedTableId >= 0)
                LoadOrder();
        }
    }
}
