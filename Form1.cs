using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TechMartProductManager
{
    public partial class Form1 : Form
    {
        private readonly BindingList<Product> products;
        private readonly BindingSource bindingSource;
        private bool isUpdatingGridSelection;

        public Form1()
        {
            InitializeComponent();

            products = new BindingList<Product>
            {
                new Product { ProductId = "SP001", ProductName = "iPhone 15 Pro", Category = "Điện thoại", UnitPrice = 25000000, Quantity = 12, AvatarPath = "" },
                new Product { ProductId = "SP002", ProductName = "Dell XPS 13", Category = "Laptop", UnitPrice = 32000000, Quantity = 8, AvatarPath = "" },
                new Product { ProductId = "SP003", ProductName = "Tai nghe AirPods", Category = "Phụ kiện", UnitPrice = 4500000, Quantity = 25, AvatarPath = "" }
            };

            bindingSource = new BindingSource { DataSource = products };
            dgvProducts.DataSource = bindingSource;

            ConfigureControls();
            BindGrid();
            UpdateStatus();
        }

        private void ConfigureControls()
        {
            var categories = new[] { "Điện thoại", "Laptop", "Phụ kiện" };
            cboCategory.Items.AddRange(categories);
            cboCategory.SelectedIndex = 0;

            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.MultiSelect = false;
            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.AllowUserToDeleteRows = false;
            dgvProducts.RowHeadersVisible = false;

            var columns = new[]
            {
                new { Header = "Mã SP", Property = nameof(Product.ProductId) },
                new { Header = "Tên SP", Property = nameof(Product.ProductName) },
                new { Header = "Danh Mục", Property = nameof(Product.Category) },
                new { Header = "Đơn Giá", Property = nameof(Product.UnitPrice) },
                new { Header = "Số Lượng", Property = nameof(Product.Quantity) }
            };

            dgvProducts.Columns.Clear();
            foreach (var c in columns)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    HeaderText = c.Header,
                    DataPropertyName = c.Property,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                };

                if (c.Property == nameof(Product.UnitPrice))
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                dgvProducts.Columns.Add(col);
            }

            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.ColumnStyles[0].Width = 35;
            tableLayoutPanelMain.ColumnStyles[1].Width = 65;

            this.MinimumSize = new Size(900, 550);
            errorProvider1.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            picAvatar.BorderStyle = BorderStyle.FixedSingle;
        }

        private void BindGrid()
        {
            dgvProducts.DataSource = bindingSource;
            dgvProducts.SelectionChanged += dgvProducts_SelectionChanged;
            txtSearch.TextChanged += txtSearch_TextChanged;
        }

        private void UpdateStatus()
        {
            toolStripStatusLabelTotal.Text = "Tổng số sản phẩm: " + products.Count;
        }

        private bool ValidateProductInput()
        {
            bool isValid = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                errorProvider1.SetError(txtProductName, "Tên sản phẩm không được để trống.");
                isValid = false;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice <= 0)
            {
                errorProvider1.SetError(txtUnitPrice, "Đơn giá phải > 0.");
                isValid = false;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                errorProvider1.SetError(txtQuantity, "Số lượng phải >= 0.");
                isValid = false;
            }

            return isValid;
        }

        private void ClearForm()
        {
            txtProductId.Clear();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            cboCategory.SelectedIndex = 0;
            picAvatar.Image = null;
            picAvatar.Tag = null;
            errorProvider1.Clear();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateProductInput())
                return;

            var product = new Product
            {
                ProductId = string.IsNullOrWhiteSpace(txtProductId.Text) ? "SP" + (products.Count + 1).ToString("D3") : txtProductId.Text.Trim(),
                ProductName = txtProductName.Text.Trim(),
                Category = cboCategory.SelectedItem?.ToString() ?? "Điện thoại",
                UnitPrice = decimal.Parse(txtUnitPrice.Text),
                Quantity = int.Parse(txtQuantity.Text),
                AvatarPath = picAvatar.Tag?.ToString() ?? string.Empty
            };

            products.Add(product);
            ClearForm();
            UpdateStatus();
            bindingSource.ResetBindings(false);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateProductInput())
                return;

            var selected = (Product)dgvProducts.CurrentRow.DataBoundItem;
            selected.ProductId = string.IsNullOrWhiteSpace(txtProductId.Text) ? selected.ProductId : txtProductId.Text.Trim();
            selected.ProductName = txtProductName.Text.Trim();
            selected.Category = cboCategory.SelectedItem?.ToString() ?? "Điện thoại";
            selected.UnitPrice = decimal.Parse(txtUnitPrice.Text);
            selected.Quantity = int.Parse(txtQuantity.Text);
            selected.AvatarPath = picAvatar.Tag?.ToString() ?? string.Empty;

            bindingSource.ResetBindings(false);
            UpdateStatus();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selected = (Product)dgvProducts.CurrentRow.DataBoundItem;
            var result = MessageBox.Show($"Bạn có chắc muốn xóa sản phẩm '{selected.ProductName}'?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                products.Remove(selected);
                ClearForm();
                UpdateStatus();
                bindingSource.ResetBindings(false);
            }
        }

        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            dialog.Title = "Chọn ảnh sản phẩm";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var filePath = dialog.FileName;
                    picAvatar.Image = Image.FromFile(filePath);
                    picAvatar.Tag = filePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể mở ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            var keyword = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                bindingSource.DataSource = products;
            }
            else
            {
                var filtered = products
                    .Where(p => p.ProductName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                bindingSource.DataSource = new BindingList<Product>(filtered);
            }

            UpdateStatus();
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null || isUpdatingGridSelection)
                return;

            var selected = (Product?)dgvProducts.CurrentRow.DataBoundItem;
            if (selected == null)
                return;

            isUpdatingGridSelection = true;

            txtProductId.Text = selected.ProductId;
            txtProductName.Text = selected.ProductName;
            cboCategory.SelectedItem = selected.Category;
            txtUnitPrice.Text = selected.UnitPrice.ToString();
            txtQuantity.Text = selected.Quantity.ToString();

            if (!string.IsNullOrWhiteSpace(selected.AvatarPath) && File.Exists(selected.AvatarPath))
            {
                picAvatar.Image = Image.FromFile(selected.AvatarPath);
                picAvatar.Tag = selected.AvatarPath;
            }
            else
            {
                picAvatar.Image = null;
                picAvatar.Tag = null;
            }

            isUpdatingGridSelection = false;
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog();
            dialog.Filter = "CSV files (*.csv)|*.csv";
            dialog.Title = "Xuất danh sách sản phẩm ra CSV";
            dialog.DefaultExt = "csv";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var writer = new StreamWriter(dialog.FileName, false);
                    writer.WriteLine("Mã SP,Tên SP,Danh Mục,Đơn Giá,Số Lượng");

                    foreach (var p in products)
                    {
                        writer.WriteLine($"{p.ProductId},{p.ProductName},{p.Category},{p.UnitPrice},{p.Quantity}");
                    }

                    MessageBox.Show("Xuất CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void exportCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnExportCsv_Click(sender, e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnExit_Click(sender, e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (dgvProducts.Rows.Count > 0)
            {
                dgvProducts.Rows[0].Selected = true;
            }
        }
    }
}
