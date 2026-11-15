using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using B2B.Core.Data;
using B2B.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2B.WinForms
{
    public class MainForm : Form
    {
        private readonly User _currentUser;
        private TabControl tabControl = null!;

        // Items Controls
        private DataGridView dgvItems = null!;
        private TextBox txtItemName = null!;
        private TextBox txtItemSize = null!;
        private NumericUpDown numItemPrice = null!;
        private NumericUpDown numItemStock = null!;
        private int selectedItemId = 0;

        // Agents Controls
        private DataGridView dgvAgents = null!;
        private TextBox txtAgentName = null!;
        private TextBox txtAgentAddress = null!;
        private TextBox txtAgentPhone = null!;
        private int selectedAgentId = 0;

        // Orders Controls
        private DataGridView dgvOrders = null!;
        private DataGridView dgvOrderDetails = null!;
        private ComboBox cboOrderStatus = null!;
        private int selectedOrderId = 0;

        // Dashboard Controls
        private DataGridView dgvBestSellers = null!;
        private DataGridView dgvLowStock = null!;
        private ComboBox cboAgentRevenue = null!;
        private Label lblTotalRevenueResult = null!;

        public MainForm(User user)
        {
            _currentUser = user;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"B2B Wholesale Back-Office System - Logged in as {_currentUser.UserName} ({_currentUser.Role})";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Menu Strip
            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var refreshItem = new ToolStripMenuItem("Refresh All Data", null, async (s, e) => await LoadAllDataAsync());
            var logoutItem = new ToolStripMenuItem("Logout", null, (s, e) => this.Close());
            fileMenu.DropDownItems.Add(refreshItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(logoutItem);
            menuStrip.Items.Add(fileMenu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Status strip
            var statusStrip = new StatusStrip();
            var lblStatusUser = new ToolStripStatusLabel($"User: {_currentUser.UserName} | Role: {_currentUser.Role} | ") { Spring = false };
            var lblCopyright = new ToolStripStatusLabel("Developed by Tran Hoang Anh (524K0015) - SE Lab Assignment 2") { Spring = true, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.DarkSlateBlue };
            statusStrip.Items.Add(lblStatusUser);
            statusStrip.Items.Add(lblCopyright);
            this.Controls.Add(statusStrip);

            // Tab Control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            tabControl.TabPages.Add(CreateItemsTabPage());
            tabControl.TabPages.Add(CreateAgentsTabPage());
            tabControl.TabPages.Add(CreateOrdersTabPage());
            tabControl.TabPages.Add(CreateDashboardTabPage());

            this.Controls.Add(tabControl);
            tabControl.BringToFront();

            this.Load += async (s, e) => await LoadAllDataAsync();
        }

        #region Tab 1: Item Management (Task 1.2)
        private TabPage CreateItemsTabPage()
        {
            var tab = new TabPage("Inventory & Items");

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 130, Padding = new Padding(10) };
            
            var lblName = new Label { Text = "Item Name:", Location = new Point(15, 15), AutoSize = true };
            txtItemName = new TextBox { Location = new Point(100, 12), Size = new Size(240, 25) };

            var lblSize = new Label { Text = "Size / Pack:", Location = new Point(360, 15), AutoSize = true };
            txtItemSize = new TextBox { Location = new Point(440, 12), Size = new Size(160, 25) };

            var lblPrice = new Label { Text = "Price (VND):", Location = new Point(15, 50), AutoSize = true };
            numItemPrice = new NumericUpDown { Location = new Point(100, 48), Size = new Size(150, 25), Maximum = 1000000000, DecimalPlaces = 0, Increment = 10000 };

            var lblStock = new Label { Text = "Stock Qty:", Location = new Point(270, 50), AutoSize = true };
            numItemStock = new NumericUpDown { Location = new Point(345, 48), Size = new Size(100, 25), Maximum = 100000, DecimalPlaces = 0, Increment = 10 };

            var btnAdd = new Button { Text = "Add Item", Location = new Point(15, 88), Size = new Size(100, 32), BackColor = Color.ForestGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnUpdate = new Button { Text = "Update", Location = new Point(125, 88), Size = new Size(90, 32), BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnDelete = new Button { Text = "Delete", Location = new Point(225, 88), Size = new Size(90, 32), BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnQuickRestock = new Button { Text = "+50 Quick Restock", Location = new Point(325, 88), Size = new Size(140, 32), BackColor = Color.DarkOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnAdd.Click += async (s, e) => await AddItemAsync();
            btnUpdate.Click += async (s, e) => await UpdateItemAsync();
            btnDelete.Click += async (s, e) => await DeleteItemAsync();
            btnQuickRestock.Click += async (s, e) => await QuickRestockItemAsync(50);

            panelTop.Controls.AddRange(new Control[] { lblName, txtItemName, lblSize, txtItemSize, lblPrice, numItemPrice, lblStock, numItemStock, btnAdd, btnUpdate, btnDelete, btnQuickRestock });

            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true
            };
            dgvItems.SelectionChanged += (s, e) =>
            {
                if (dgvItems.SelectedRows.Count > 0)
                {
                    var row = dgvItems.SelectedRows[0];
                    selectedItemId = Convert.ToInt32(row.Cells["ItemID"].Value);
                    txtItemName.Text = row.Cells["ItemName"].Value?.ToString() ?? "";
                    txtItemSize.Text = row.Cells["Size"].Value?.ToString() ?? "";
                    numItemPrice.Value = Convert.ToDecimal(row.Cells["Price"].Value);
                    numItemStock.Value = Convert.ToDecimal(row.Cells["StockQuantity"].Value);
                }
            };

            tab.Controls.Add(dgvItems);
            tab.Controls.Add(panelTop);
            return tab;
        }

        private async Task LoadItemsAsync()
        {
            try
            {
                using var context = DbContextFactory.CreateDbContext();
                var items = await context.Items.OrderBy(i => i.ItemID).ToListAsync();
                dgvItems.DataSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddItemAsync()
        {
            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                MessageBox.Show("Item name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var context = DbContextFactory.CreateDbContext();
            var item = new Item
            {
                ItemName = txtItemName.Text.Trim(),
                Size = txtItemSize.Text.Trim(),
                Price = numItemPrice.Value,
                StockQuantity = (int)numItemStock.Value
            };
            context.Items.Add(item);
            await context.SaveChangesAsync();
            await LoadItemsAsync();
            MessageBox.Show("New item added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task UpdateItemAsync()
        {
            if (selectedItemId <= 0) return;
            using var context = DbContextFactory.CreateDbContext();
            var item = await context.Items.FindAsync(selectedItemId);
            if (item != null)
            {
                item.ItemName = txtItemName.Text.Trim();
                item.Size = txtItemSize.Text.Trim();
                item.Price = numItemPrice.Value;
                item.StockQuantity = (int)numItemStock.Value;
                await context.SaveChangesAsync();
                await LoadItemsAsync();
                MessageBox.Show("Item updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task DeleteItemAsync()
        {
            if (selectedItemId <= 0) return;
            var confirm = MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                using var context = DbContextFactory.CreateDbContext();
                var item = await context.Items.FindAsync(selectedItemId);
                if (item != null)
                {
                    context.Items.Remove(item);
                    await context.SaveChangesAsync();
                    await LoadItemsAsync();
                    MessageBox.Show("Item deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot delete item because it has associated orders: {ex.Message}", "Integrity Constraint", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task QuickRestockItemAsync(int amount)
        {
            if (selectedItemId <= 0) return;
            using var context = DbContextFactory.CreateDbContext();
            var item = await context.Items.FindAsync(selectedItemId);
            if (item != null)
            {
                item.StockQuantity += amount;
                await context.SaveChangesAsync();
                await LoadItemsAsync();
                MessageBox.Show($"Restocked {amount} units. New stock: {item.StockQuantity}", "Restocked", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Tab 2: Agent Management (Task 1.2)
        private TabPage CreateAgentsTabPage()
        {
            var tab = new TabPage("Wholesale Agents");

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(10) };
            
            var lblName = new Label { Text = "Agent Name:", Location = new Point(15, 15), AutoSize = true };
            txtAgentName = new TextBox { Location = new Point(105, 12), Size = new Size(240, 25) };

            var lblAddress = new Label { Text = "Address:", Location = new Point(365, 15), AutoSize = true };
            txtAgentAddress = new TextBox { Location = new Point(430, 12), Size = new Size(300, 25) };

            var lblPhone = new Label { Text = "Phone:", Location = new Point(745, 15), AutoSize = true };
            txtAgentPhone = new TextBox { Location = new Point(800, 12), Size = new Size(150, 25) };

            var btnAdd = new Button { Text = "Register Agent", Location = new Point(105, 55), Size = new Size(130, 32), BackColor = Color.ForestGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnUpdate = new Button { Text = "Update", Location = new Point(245, 55), Size = new Size(90, 32), BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnDelete = new Button { Text = "Delete", Location = new Point(345, 55), Size = new Size(90, 32), BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnAdd.Click += async (s, e) => await AddAgentAsync();
            btnUpdate.Click += async (s, e) => await UpdateAgentAsync();
            btnDelete.Click += async (s, e) => await DeleteAgentAsync();

            panelTop.Controls.AddRange(new Control[] { lblName, txtAgentName, lblAddress, txtAgentAddress, lblPhone, txtAgentPhone, btnAdd, btnUpdate, btnDelete });

            dgvAgents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true
            };
            dgvAgents.SelectionChanged += (s, e) =>
            {
                if (dgvAgents.SelectedRows.Count > 0)
                {
                    var row = dgvAgents.SelectedRows[0];
                    selectedAgentId = Convert.ToInt32(row.Cells["AgentID"].Value);
                    txtAgentName.Text = row.Cells["AgentName"].Value?.ToString() ?? "";
                    txtAgentAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";
                    txtAgentPhone.Text = row.Cells["Phone"].Value?.ToString() ?? "";
                }
            };

            tab.Controls.Add(dgvAgents);
            tab.Controls.Add(panelTop);
            return tab;
        }

        private async Task LoadAgentsAsync()
        {
            try
            {
                using var context = DbContextFactory.CreateDbContext();
                var agents = await context.Agents.OrderBy(a => a.AgentID).ToListAsync();
                dgvAgents.DataSource = agents;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load agents: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddAgentAsync()
        {
            if (string.IsNullOrWhiteSpace(txtAgentName.Text))
            {
                MessageBox.Show("Agent name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var context = DbContextFactory.CreateDbContext();
            var agent = new Agent
            {
                AgentName = txtAgentName.Text.Trim(),
                Address = txtAgentAddress.Text.Trim(),
                Phone = txtAgentPhone.Text.Trim()
            };
            context.Agents.Add(agent);
            await context.SaveChangesAsync();
            await LoadAgentsAsync();
            MessageBox.Show("Agent registered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task UpdateAgentAsync()
        {
            if (selectedAgentId <= 0) return;
            using var context = DbContextFactory.CreateDbContext();
            var agent = await context.Agents.FindAsync(selectedAgentId);
            if (agent != null)
            {
                agent.AgentName = txtAgentName.Text.Trim();
                agent.Address = txtAgentAddress.Text.Trim();
                agent.Phone = txtAgentPhone.Text.Trim();
                await context.SaveChangesAsync();
                await LoadAgentsAsync();
                MessageBox.Show("Agent updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task DeleteAgentAsync()
        {
            if (selectedAgentId <= 0) return;
            var confirm = MessageBox.Show("Deleting this agent will cascade delete all associated orders. Proceed?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using var context = DbContextFactory.CreateDbContext();
            var agent = await context.Agents.FindAsync(selectedAgentId);
            if (agent != null)
            {
                context.Agents.Remove(agent);
                await context.SaveChangesAsync();
                await LoadAgentsAsync();
                MessageBox.Show("Agent deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Tab 3: Order Management & Shipping Manifest (Task 1.2 & 1.3)
        private TabPage CreateOrdersTabPage()
        {
            var tab = new TabPage("Incoming Orders & Manifest");

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };
            
            var lblStatus = new Label { Text = "Update Status:", Location = new Point(15, 18), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cboOrderStatus = new ComboBox { Location = new Point(125, 15), Size = new Size(160, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboOrderStatus.Items.AddRange(new object[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" });
            cboOrderStatus.SelectedIndex = 0;

            var btnUpdateStatus = new Button { Text = "Save Order Status", Location = new Point(295, 12), Size = new Size(150, 32), BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnPrintManifest = new Button { Text = "🖨️ Print Shipping Manifest", Location = new Point(460, 12), Size = new Size(200, 32), BackColor = Color.DarkSlateGray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };

            btnUpdateStatus.Click += async (s, e) => await UpdateOrderStatusAsync();
            btnPrintManifest.Click += async (s, e) => await GenerateShippingManifestAsync();

            panelTop.Controls.AddRange(new Control[] { lblStatus, cboOrderStatus, btnUpdateStatus, btnPrintManifest });

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 260
            };

            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true
            };
            dgvOrders.SelectionChanged += async (s, e) =>
            {
                if (dgvOrders.SelectedRows.Count > 0)
                {
                    var row = dgvOrders.SelectedRows[0];
                    selectedOrderId = Convert.ToInt32(row.Cells["OrderID"].Value);
                    var status = row.Cells["OrderStatus"].Value?.ToString();
                    if (!string.IsNullOrEmpty(status) && cboOrderStatus.Items.Contains(status))
                    {
                        cboOrderStatus.SelectedItem = status;
                    }
                    await LoadOrderDetailsAsync(selectedOrderId);
                }
            };

            dgvOrderDetails = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            splitContainer.Panel1.Controls.Add(dgvOrders);
            splitContainer.Panel2.Controls.Add(dgvOrderDetails);

            tab.Controls.Add(splitContainer);
            tab.Controls.Add(panelTop);
            return tab;
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                using var context = DbContextFactory.CreateDbContext();
                var orders = await context.Orders
                    .Include(o => o.Agent)
                    .Select(o => new
                    {
                        o.OrderID,
                        o.OrderDate,
                        AgentName = o.Agent != null ? o.Agent.AgentName : "N/A",
                        o.OrderStatus,
                        TotalLines = o.OrderDetails.Count,
                        TotalValue = o.OrderDetails.Sum(d => d.Quantity * d.UnitAmount)
                    })
                    .OrderByDescending(o => o.OrderID)
                    .ToListAsync();

                dgvOrders.DataSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load orders: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadOrderDetailsAsync(int orderId)
        {
            try
            {
                using var context = DbContextFactory.CreateDbContext();
                var details = await context.OrderDetails
                    .Where(d => d.OrderID == orderId)
                    .Include(d => d.Item)
                    .Select(d => new
                    {
                        d.ID,
                        d.ItemID,
                        ItemName = d.Item != null ? d.Item.ItemName : "N/A",
                        Size = d.Item != null ? d.Item.Size : "N/A",
                        d.Quantity,
                        d.UnitAmount,
                        Subtotal = d.Quantity * d.UnitAmount
                    })
                    .ToListAsync();

                dgvOrderDetails.DataSource = details;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load order details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateOrderStatusAsync()
        {
            if (selectedOrderId <= 0)
            {
                MessageBox.Show("Please select an order first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var context = DbContextFactory.CreateDbContext();
            var order = await context.Orders.FindAsync(selectedOrderId);
            if (order != null)
            {
                order.OrderStatus = cboOrderStatus.SelectedItem?.ToString() ?? "Pending";
                await context.SaveChangesAsync();
                await LoadOrdersAsync();
                MessageBox.Show($"Order #{selectedOrderId} status updated to {order.OrderStatus}!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task GenerateShippingManifestAsync()
        {
            if (selectedOrderId <= 0)
            {
                MessageBox.Show("Please select an order to generate shipping manifest.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var context = DbContextFactory.CreateDbContext();
            var order = await context.Orders
                .Include(o => o.Agent)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(o => o.OrderID == selectedOrderId);

            if (order == null) return;

            var manifestText = "======================================================================\n" +
                               "                    B2B WHOLESALE SYSTEM                             \n" +
                               "                    OFFICIAL SHIPPING MANIFEST                        \n" +
                               "======================================================================\n" +
                              $"Manifest Date:      {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                              $"Order Number:       #{order.OrderID}\n" +
                              $"Order Placement:    {order.OrderDate:yyyy-MM-dd HH:mm:ss}\n" +
                              $"Current Status:     {order.OrderStatus}\n" +
                               "----------------------------------------------------------------------\n" +
                               "RECIPIENT / AGENT INFORMATION:\n" +
                              $"Agent Name:         {order.Agent?.AgentName}\n" +
                              $"Shipping Address:   {order.Agent?.Address}\n" +
                              $"Contact Phone:      {order.Agent?.Phone}\n" +
                               "----------------------------------------------------------------------\n" +
                               "SHIPPED ITEMS LIST:\n" +
                               string.Format("{0,-8} {1,-35} {2,-10} {3,8} {4,14}\n", "ItemID", "Item Description", "Size", "Quantity", "Unit Price") +
                               "----------------------------------------------------------------------\n";

            decimal totalAmount = 0;
            int totalUnits = 0;

            foreach (var d in order.OrderDetails)
            {
                var itemName = d.Item?.ItemName ?? "Unknown Item";
                if (itemName.Length > 34) itemName = itemName.Substring(0, 31) + "...";
                var itemSize = d.Item?.Size ?? "-";
                manifestText += string.Format("{0,-8} {1,-35} {2,-10} {3,8} {4,14:N0} VND\n", d.ItemID, itemName, itemSize, d.Quantity, d.UnitAmount);
                totalAmount += d.Quantity * d.UnitAmount;
                totalUnits += d.Quantity;
            }

            manifestText += "----------------------------------------------------------------------\n" +
                           $"TOTAL UNITS SHIPPED:  {totalUnits} Units\n" +
                           $"TOTAL INVOICE VALUE:  {totalAmount:N0} VND\n" +
                            "----------------------------------------------------------------------\n" +
                            "WAREHOUSE DISPATCH VERIFICATION:\n" +
                            "Prepared By:        ____________________    Date: __________________\n" +
                            "Carrier Signature:  ____________________    Plate: _________________\n" +
                            "----------------------------------------------------------------------\n";

            // Save to temporary file and open in default viewer / print preview
            var tempFile = Path.Combine(Path.GetTempPath(), $"Shipping_Manifest_Order_{order.OrderID}.txt");
            await File.WriteAllTextAsync(tempFile, manifestText);

            var previewForm = new Form
            {
                Text = $"Shipping Manifest - Order #{order.OrderID}",
                Size = new Size(720, 600),
                StartPosition = FormStartPosition.CenterParent
            };
            var txtBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                Text = manifestText,
                ScrollBars = ScrollBars.Vertical
            };
            previewForm.Controls.Add(txtBox);
            previewForm.ShowDialog();
        }
        #endregion

        #region Tab 4: Analytics & Dashboard (Task 1.4)
        private TabPage CreateDashboardTabPage()
        {
            var tab = new TabPage("Sales Dashboard & Analytics");

            var splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 540
            };

            // Left Panel: Best Selling Items & Revenue by Agent
            var panelLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var lblBestTitle = new Label { Text = "📊 Top 5 Best-Selling Items (By Quantity Sold)", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold) };
            dgvBestSellers = new DataGridView { Dock = DockStyle.Top, Height = 220, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };

            var panelRev = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 15, 0, 0) };
            var lblRevTitle = new Label { Text = "💰 Revenue Analysis by Distributor Agent", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold) };
            
            var panelRevControls = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblAgentSelect = new Label { Text = "Select Agent:", Location = new Point(0, 12), AutoSize = true };
            cboAgentRevenue = new ComboBox { Location = new Point(95, 8), Size = new Size(240, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboAgentRevenue.SelectedIndexChanged += async (s, e) => await CalculateAgentRevenueAsync();
            lblTotalRevenueResult = new Label { Text = "Total Revenue: 0 VND", Location = new Point(0, 38), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DarkGreen };
            panelRevControls.Controls.AddRange(new Control[] { lblAgentSelect, cboAgentRevenue });

            panelRev.Controls.Add(lblTotalRevenueResult);
            panelRev.Controls.Add(panelRevControls);
            panelRev.Controls.Add(lblRevTitle);

            panelLeft.Controls.Add(panelRev);
            panelLeft.Controls.Add(dgvBestSellers);
            panelLeft.Controls.Add(lblBestTitle);

            // Right Panel: Low Stock Alert Warning
            var panelRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var lblLowTitle = new Label { Text = "⚠️ Low-Stock Items Alert (< 200 units in stock)", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Color.DarkRed };
            dgvLowStock = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            panelRight.Controls.Add(dgvLowStock);
            panelRight.Controls.Add(lblLowTitle);

            splitMain.Panel1.Controls.Add(panelLeft);
            splitMain.Panel2.Controls.Add(panelRight);

            tab.Controls.Add(splitMain);
            return tab;
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                using var context = DbContextFactory.CreateDbContext();

                // 1. Top 5 Best-Selling items
                var bestSellers = await context.OrderDetails
                    .Include(d => d.Item)
                    .GroupBy(d => new { d.ItemID, ItemName = d.Item != null ? d.Item.ItemName : "N/A" })
                    .Select(g => new
                    {
                        g.Key.ItemID,
                        g.Key.ItemName,
                        TotalQuantitySold = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.Quantity * x.UnitAmount)
                    })
                    .OrderByDescending(x => x.TotalQuantitySold)
                    .Take(5)
                    .ToListAsync();

                dgvBestSellers.DataSource = bestSellers;

                // 2. Low-stock items (< 200 units threshold)
                var lowStock = await context.Items
                    .Where(i => i.StockQuantity < 200)
                    .Select(i => new
                    {
                        i.ItemID,
                        i.ItemName,
                        i.Size,
                        i.Price,
                        i.StockQuantity,
                        Status = i.StockQuantity <= 50 ? "CRITICAL" : "LOW"
                    })
                    .OrderBy(i => i.StockQuantity)
                    .ToListAsync();

                dgvLowStock.DataSource = lowStock;

                // 3. Populate Agent Revenue ComboBox
                var agents = await context.Agents.OrderBy(a => a.AgentName).ToListAsync();
                cboAgentRevenue.DisplayMember = "AgentName";
                cboAgentRevenue.ValueMember = "AgentID";
                cboAgentRevenue.DataSource = agents;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load dashboard data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CalculateAgentRevenueAsync()
        {
            if (cboAgentRevenue.SelectedValue is int agentId)
            {
                using var context = DbContextFactory.CreateDbContext();
                var revenue = await context.OrderDetails
                    .Where(d => d.Order != null && d.Order.AgentID == agentId && d.Order.OrderStatus != "Cancelled")
                    .SumAsync(d => (decimal?)(d.Quantity * d.UnitAmount)) ?? 0m;

                var orderCount = await context.Orders
                    .Where(o => o.AgentID == agentId && o.OrderStatus != "Cancelled")
                    .CountAsync();

                lblTotalRevenueResult.Text = $"Total Revenue: {revenue:N0} VND across {orderCount} orders";
            }
        }
        #endregion

        private async Task LoadAllDataAsync()
        {
            await LoadItemsAsync();
            await LoadAgentsAsync();
            await LoadOrdersAsync();
            await LoadDashboardDataAsync();
        }
    }
}
