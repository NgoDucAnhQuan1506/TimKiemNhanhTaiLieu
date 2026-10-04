using Sunny.UI;
using System.Windows.Forms;

namespace TimKiemNhanhTaiLieu
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBox1 = new Sunny.UI.UIGroupBox();
            this.label3 = new Sunny.UI.UILabel();
            this.cbSearchMode = new Sunny.UI.UIComboBox();
            this.btnSelect = new Sunny.UI.UIButton();
            this.txtPath = new Sunny.UI.UITextBox();
            this.label2 = new Sunny.UI.UILabel();
            this.line1 = new Sunny.UI.UILine();
            this.btnSearch = new Sunny.UI.UIButton();
            this.txtSearch = new Sunny.UI.UITextBox();
            this.label1 = new Sunny.UI.UILabel();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.dataFile = new Sunny.UI.UIDataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnFileIcon = new System.Windows.Forms.DataGridViewImageColumn();
            this.TenTaiLieu = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ViTri = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.line2 = new Sunny.UI.UILine();
            this.lblResult = new Sunny.UI.UILabel();
            this.btnOpen = new Sunny.UI.UIButton();
            this.btn_map = new Sunny.UI.UIButton();
            this.btndelete = new Sunny.UI.UIButton();
            this.btnOut = new Sunny.UI.UIButton();
            this.UIGroupBox = new Sunny.UI.UIGroupBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataFile)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cbSearchMode);
            this.groupBox1.Controls.Add(this.btnSelect);
            this.groupBox1.Controls.Add(this.txtPath);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.line1);
            this.groupBox1.Controls.Add(this.btnSearch);
            this.groupBox1.Controls.Add(this.txtSearch);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Times New Roman", 12F);
            this.groupBox1.Location = new System.Drawing.Point(12, 42);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.groupBox1.Size = new System.Drawing.Size(782, 143);
            this.groupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.groupBox1.TabIndex = 0;
            this.groupBox1.Text = "Thông tin tìm kiếm";
            this.groupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.label3.Location = new System.Drawing.Point(7, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 25);
            this.label3.TabIndex = 8;
            this.label3.Text = "Phạm vi tìm kiếm:";
            // 
            // cbSearchMode
            // 
            this.cbSearchMode.DataSource = null;
            this.cbSearchMode.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cbSearchMode.FillColor = System.Drawing.Color.White;
            this.cbSearchMode.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbSearchMode.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.cbSearchMode.Items.AddRange(new object[] {
            "Tên file",
            "Nội dung file",
            "Tên file và nội dung"});
            this.cbSearchMode.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cbSearchMode.Location = new System.Drawing.Point(188, 75);
            this.cbSearchMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbSearchMode.MinimumSize = new System.Drawing.Size(63, 0);
            this.cbSearchMode.Name = "cbSearchMode";
            this.cbSearchMode.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cbSearchMode.Size = new System.Drawing.Size(215, 34);
            this.cbSearchMode.SymbolSize = 24;
            this.cbSearchMode.TabIndex = 7;
            this.cbSearchMode.Text = "Chức năng nè";
            this.cbSearchMode.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cbSearchMode.Watermark = "";
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnSelect.Location = new System.Drawing.Point(688, 105);
            this.btnSelect.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(81, 34);
            this.btnSelect.TabIndex = 6;
            this.btnSelect.Text = "📁 Chọn";
            this.btnSelect.TipsFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPath.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPath.Location = new System.Drawing.Point(188, 112);
            this.txtPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPath.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtPath.Name = "txtPath";
            this.txtPath.Padding = new System.Windows.Forms.Padding(5);
            this.txtPath.ReadOnly = true;
            this.txtPath.ShowText = false;
            this.txtPath.Size = new System.Drawing.Size(483, 26);
            this.txtPath.TabIndex = 5;
            this.txtPath.Text = "Ổ đĩa ở đây nè";
            this.txtPath.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtPath.Watermark = "";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.label2.Location = new System.Drawing.Point(5, 109);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(174, 25);
            this.label2.TabIndex = 4;
            this.label2.Text = "Tìm trong thư mục:";
            // 
            // line1
            // 
            this.line1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.line1.BackColor = System.Drawing.Color.Transparent;
            this.line1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.line1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.line1.Location = new System.Drawing.Point(0, 67);
            this.line1.MinimumSize = new System.Drawing.Size(1, 1);
            this.line1.Name = "line1";
            this.line1.Size = new System.Drawing.Size(782, 13);
            this.line1.TabIndex = 3;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnSearch.Location = new System.Drawing.Point(652, 30);
            this.btnSearch.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(102, 31);
            this.btnSearch.Style = Sunny.UI.UIStyle.Custom;
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "🔍Tìm kiếm";
            this.btnSearch.TipsFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtSearch.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.Location = new System.Drawing.Point(95, 30);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtSearch.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Padding = new System.Windows.Forms.Padding(5);
            this.txtSearch.ShowText = false;
            this.txtSearch.Size = new System.Drawing.Size(539, 31);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtSearch.Watermark = "Nhập từ khóa cần tìm...";
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.label1.Location = new System.Drawing.Point(7, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Từ khóa:";
            // 
            // dataFile
            // 
            this.dataFile.AllowUserToAddRows = false;
            this.dataFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataFile.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataFile.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.dataFile.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Times New Roman", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataFile.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataFile.ColumnHeadersHeight = 38;
            this.dataFile.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataFile.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.ColumnFileIcon,
            this.TenTaiLieu,
            this.ViTri,
            this.Column3,
            this.Column4});
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Times New Roman", 12F);
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataFile.DefaultCellStyle = dataGridViewCellStyle7;
            this.dataFile.EnableHeadersVisualStyles = false;
            this.dataFile.Font = new System.Drawing.Font("Times New Roman", 12F);
            this.dataFile.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(173)))), ((int)(((byte)(255)))));
            this.dataFile.Location = new System.Drawing.Point(12, 193);
            this.dataFile.Name = "dataFile";
            this.dataFile.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Times New Roman", 12F);
            dataGridViewCellStyle8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataFile.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            this.dataFile.RowHeadersWidth = 51;
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.dataFile.RowsDefaultCellStyle = dataGridViewCellStyle9;
            this.dataFile.RowTemplate.Height = 34;
            this.dataFile.SelectedIndex = -1;
            this.dataFile.Size = new System.Drawing.Size(782, 239);
            this.dataFile.StripeOddColor = System.Drawing.Color.Empty;
            this.dataFile.Style = Sunny.UI.UIStyle.Custom;
            this.dataFile.TabIndex = 1;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle2;
            this.Column1.HeaderText = "STT";
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.Width = 73;
            // 
            // ColumnFileIcon
            // 
            this.ColumnFileIcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.NullValue = null;
            this.ColumnFileIcon.DefaultCellStyle = dataGridViewCellStyle3;
            this.ColumnFileIcon.HeaderText = "Loại File";
            this.ColumnFileIcon.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.ColumnFileIcon.MinimumWidth = 40;
            this.ColumnFileIcon.Name = "ColumnFileIcon";
            this.ColumnFileIcon.ReadOnly = true;
            this.ColumnFileIcon.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnFileIcon.Width = 88;
            // 
            // TenTaiLieu
            // 
            this.TenTaiLieu.DefaultCellStyle = dataGridViewCellStyle2;
            this.TenTaiLieu.HeaderText = "Tên Tài Liệu";
            this.TenTaiLieu.MinimumWidth = 6;
            this.TenTaiLieu.Name = "TenTaiLieu";
            // 
            // ViTri
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.ViTri.DefaultCellStyle = dataGridViewCellStyle4;
            this.ViTri.HeaderText = "Vị trí";
            this.ViTri.MinimumWidth = 6;
            this.ViTri.Name = "ViTri";
            // 
            // Column3
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column3.DefaultCellStyle = dataGridViewCellStyle5;
            this.Column3.HeaderText = "Kích Thước";
            this.Column3.MinimumWidth = 6;
            this.Column3.Name = "Column3";
            // 
            // Column4
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column4.DefaultCellStyle = dataGridViewCellStyle6;
            this.Column4.HeaderText = "Ngày Sửa Đổi";
            this.Column4.MinimumWidth = 6;
            this.Column4.Name = "Column4";
            // 
            // line2
            // 
            this.line2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.line2.BackColor = System.Drawing.Color.Transparent;
            this.line2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.line2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.line2.Location = new System.Drawing.Point(12, 460);
            this.line2.MinimumSize = new System.Drawing.Size(1, 1);
            this.line2.Name = "line2";
            this.line2.Size = new System.Drawing.Size(782, 6);
            this.line2.TabIndex = 4;
            // 
            // lblResult
            // 
            this.lblResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold);
            this.lblResult.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.lblResult.Location = new System.Drawing.Point(9, 435);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(162, 23);
            this.lblResult.TabIndex = 5;
            this.lblResult.Text = "Kết quả tìm kiếm:";
            this.lblResult.Click += new System.EventHandler(this.lblResult_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOpen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpen.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold);
            this.btnOpen.Location = new System.Drawing.Point(303, 474);
            this.btnOpen.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(112, 33);
            this.btnOpen.Style = Sunny.UI.UIStyle.Custom;
            this.btnOpen.TabIndex = 7;
            this.btnOpen.Text = "📄 Mở";
            this.btnOpen.TipsFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btn_map
            // 
            this.btn_map.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_map.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_map.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.btn_map.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.btn_map.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(203)))), ((int)(((byte)(83)))));
            this.btn_map.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(152)))), ((int)(((byte)(32)))));
            this.btn_map.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(152)))), ((int)(((byte)(32)))));
            this.btn_map.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold);
            this.btn_map.LightColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(251)))), ((int)(((byte)(241)))));
            this.btn_map.Location = new System.Drawing.Point(430, 474);
            this.btn_map.MinimumSize = new System.Drawing.Size(1, 1);
            this.btn_map.Name = "btn_map";
            this.btn_map.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.btn_map.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(203)))), ((int)(((byte)(83)))));
            this.btn_map.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(152)))), ((int)(((byte)(32)))));
            this.btn_map.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(152)))), ((int)(((byte)(32)))));
            this.btn_map.Size = new System.Drawing.Size(112, 33);
            this.btn_map.Style = Sunny.UI.UIStyle.Custom;
            this.btn_map.TabIndex = 8;
            this.btn_map.Text = "📍Mở vị trí";
            this.btn_map.TipsFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btn_map.Click += new System.EventHandler(this.btn_map_Click);
            // 
            // btndelete
            // 
            this.btndelete.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btndelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btndelete.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btndelete.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btndelete.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.btndelete.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btndelete.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btndelete.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold);
            this.btndelete.LightColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.btndelete.Location = new System.Drawing.Point(561, 474);
            this.btndelete.MinimumSize = new System.Drawing.Size(1, 1);
            this.btndelete.Name = "btndelete";
            this.btndelete.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btndelete.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.btndelete.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btndelete.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btndelete.Size = new System.Drawing.Size(112, 33);
            this.btndelete.Style = Sunny.UI.UIStyle.Custom;
            this.btndelete.TabIndex = 9;
            this.btndelete.Text = "🗑 Xóa";
            this.btndelete.TipsFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btndelete.Click += new System.EventHandler(this.btndelete_Click);
            // 
            // btnOut
            // 
            this.btnOut.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOut.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.btnOut.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.btnOut.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(163)))), ((int)(((byte)(163)))), ((int)(((byte)(163)))));
            this.btnOut.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(112)))), ((int)(((byte)(112)))), ((int)(((byte)(112)))));
            this.btnOut.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(112)))), ((int)(((byte)(112)))), ((int)(((byte)(112)))));
            this.btnOut.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold);
            this.btnOut.LightColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.btnOut.Location = new System.Drawing.Point(687, 474);
            this.btnOut.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnOut.Name = "btnOut";
            this.btnOut.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.btnOut.RectHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(163)))), ((int)(((byte)(163)))), ((int)(((byte)(163)))));
            this.btnOut.RectPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(112)))), ((int)(((byte)(112)))), ((int)(((byte)(112)))));
            this.btnOut.RectSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(112)))), ((int)(((byte)(112)))), ((int)(((byte)(112)))));
            this.btnOut.Size = new System.Drawing.Size(112, 33);
            this.btnOut.Style = Sunny.UI.UIStyle.Custom;
            this.btnOut.TabIndex = 10;
            this.btnOut.Text = "🚪 Thoát";
            this.btnOut.TipsFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnOut.Click += new System.EventHandler(this.btnOut_Click);
            // 
            // UIGroupBox
            // 
            this.UIGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UIGroupBox.Font = new System.Drawing.Font("Times New Roman", 9F);
            this.UIGroupBox.Location = new System.Drawing.Point(12, 35);
            this.UIGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.UIGroupBox.MinimumSize = new System.Drawing.Size(1, 1);
            this.UIGroupBox.Name = "UIGroupBox";
            this.UIGroupBox.Padding = new System.Windows.Forms.Padding(4, 32, 4, 2);
            this.UIGroupBox.Size = new System.Drawing.Size(270, 180);
            this.UIGroupBox.TabIndex = 0;
            this.UIGroupBox.Text = null;
            this.UIGroupBox.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(836, 517);
            this.Controls.Add(this.btnOut);
            this.Controls.Add(this.btndelete);
            this.Controls.Add(this.btn_map);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.line2);
            this.Controls.Add(this.dataFile);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "Công cụ tìm kiếm nhanh tài liệu";
            this.ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 836, 517);
            this.Load += new System.EventHandler(this.FileForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataFile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Sunny.UI.UIGroupBox groupBox1;
        private Sunny.UI.UILabel label1;
        private Sunny.UI.UITextBox txtSearch;
        private Sunny.UI.UIButton btnSearch;
        private Sunny.UI.UILine line1;
        private Sunny.UI.UITextBox txtPath;
        private Sunny.UI.UILabel label2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private Sunny.UI.UIButton btnSelect;
        private Sunny.UI.UIDataGridView dataFile;
        private Sunny.UI.UILine line2;
        private Sunny.UI.UILabel lblResult;
        private Sunny.UI.UIButton btnOpen;
        private Sunny.UI.UIButton btn_map;
        private Sunny.UI.UIButton btndelete;
        private Sunny.UI.UIButton btnOut;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn TenTaiLieu;
        private System.Windows.Forms.DataGridViewTextBoxColumn ViTri;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private Sunny.UI.UILabel label3;
        private Sunny.UI.UIComboBox cbSearchMode;
        private UIGroupBox UIGroupBox;

        private System.Windows.Forms.DataGridViewImageColumn ColumnFileIcon;
    }
}
