using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TimKiemNhanhTaiLieu
{
    /// <summary>
    /// Form cài đặt: cho phép chọn phạm vi quét (toàn bộ ổ đĩa / chỉ tài liệu), bật/tắt từng
    /// đuôi file trong whitelist, và quản lý danh sách thư mục bị loại trừ khi quét.
    ///
    /// Toàn bộ control được tạo bằng CODE (không dùng file .Designer.cs riêng) - cùng phong
    /// cách với overlayPanel/pagingPanel đã được tạo trong Form1, để không phải đụng vào công
    /// cụ thiết kế (Designer) và giảm rủi ro xung đột khi chỉnh sửa.
    /// </summary>
    public class SettingsForm : Form
    {
        private RadioButton _radioAllFiles;
        private RadioButton _radioDocumentsOnly;
        private CheckedListBox _extensionsList;
        private ListBox _excludedFoldersList;
        private TextBox _txtNewFolder;
        private Button _btnAddFolder;
        private Button _btnRemoveFolder;
        private Button _btnOk;
        private Button _btnCancel;

        // Toàn bộ đuôi file có thể bật/tắt trong whitelist. Khớp với danh sách định dạng mà
        // LuceneSearchService.ExtractContent() có hỗ trợ trích xuất nội dung - bật thêm đuôi
        // ngoài danh sách này vẫn quét được (file vẫn được lập chỉ mục theo TÊN), chỉ là nội
        // dung sẽ không bao giờ được trích xuất/tìm thấy.
        private static readonly string[] AllKnownExtensions =
        {
            ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt",
            ".rtf", ".html", ".htm", ".json", ".pdf",
            ".txt", ".md", ".csv", ".log"
        };

        /// <summary>
        /// Cấu hình kết quả sau khi người dùng bấm "Lưu". Chỉ có giá trị hợp lệ khi
        /// ShowDialog() trả về DialogResult.OK - Form1 đọc property này ngay sau đó.
        /// </summary>
        public AppSettings ResultSettings { get; private set; }

        public SettingsForm(AppSettings current)
        {
            Text = "Cài đặt";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 494);

            BuildUi(current);
        }

        private void BuildUi(AppSettings current)
        {
            var lblMode = new Label
            {
                Text = "Phạm vi quét:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 16)
            };

            _radioAllFiles = new RadioButton
            {
                Text = "Quét toàn bộ file trong thư mục/ổ đĩa",
                AutoSize = true,
                Location = new Point(16, 44)
            };

            _radioDocumentsOnly = new RadioButton
            {
                Text = "Chỉ quét file tài liệu (theo danh sách bên dưới)",
                AutoSize = true,
                Location = new Point(16, 68)
            };

            if (current.Mode == ScanMode.AllFiles)
                _radioAllFiles.Checked = true;
            else
                _radioDocumentsOnly.Checked = true;

            var lblExt = new Label
            {
                Text = "Các loại file tài liệu được quét:",
                AutoSize = true,
                Location = new Point(16, 100)
            };

            _extensionsList = new CheckedListBox
            {
                Location = new Point(16, 124),
                Size = new Size(388, 160),
                CheckOnClick = true
            };
            foreach (string ext in AllKnownExtensions)
            {
                bool isChecked = current.AllowedExtensions
                    .Any(x => string.Equals(x, ext, StringComparison.OrdinalIgnoreCase));
                _extensionsList.Items.Add(ext, isChecked);
            }
            // Danh sách đuôi file chỉ có tác dụng ở chế độ "Chỉ quét tài liệu" - làm mờ đi khi
            // đang chọn "Quét toàn bộ" để tránh gây hiểu nhầm là nó vẫn đang lọc.
            _extensionsList.Enabled = _radioDocumentsOnly.Checked;
            _radioAllFiles.CheckedChanged += (s, e) => _extensionsList.Enabled = !_radioAllFiles.Checked;
            _radioDocumentsOnly.CheckedChanged += (s, e) => _extensionsList.Enabled = _radioDocumentsOnly.Checked;

            var lblFolders = new Label
            {
                Text = "Thư mục bị loại trừ khi quét (áp dụng cho cả 2 chế độ):",
                AutoSize = true,
                Location = new Point(16, 294)
            };

            _excludedFoldersList = new ListBox
            {
                Location = new Point(16, 318),
                Size = new Size(388, 90)
            };
            foreach (string folder in current.ExcludedFolderNames)
                _excludedFoldersList.Items.Add(folder);

            var lblNewFolder = new Label
            {
                Text = "Tên thư mục mới (gõ tên, không phải đường dẫn đầy đủ):",
                AutoSize = true,
                Location = new Point(16, 414)
            };

            _txtNewFolder = new TextBox
            {
                Location = new Point(16, 436),
                Size = new Size(220, 24)
            };

            _btnAddFolder = new Button
            {
                Text = "+ Thêm",
                Location = new Point(242, 434),
                Size = new Size(80, 28)
            };
            _btnAddFolder.Click += (s, e) =>
            {
                string name = _txtNewFolder.Text.Trim();
                if (string.IsNullOrEmpty(name)) return;

                bool alreadyExists = _excludedFoldersList.Items.Cast<string>()
                    .Any(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
                if (!alreadyExists)
                    _excludedFoldersList.Items.Add(name);

                _txtNewFolder.Clear();
                _txtNewFolder.Focus();
            };

            _btnRemoveFolder = new Button
            {
                Text = "- Xóa",
                Location = new Point(324, 434),
                Size = new Size(80, 28)
            };
            _btnRemoveFolder.Click += (s, e) =>
            {
                if (_excludedFoldersList.SelectedIndex >= 0)
                    _excludedFoldersList.Items.RemoveAt(_excludedFoldersList.SelectedIndex);
            };

            _btnOk = new Button
            {
                Text = "Lưu",
                Location = new Point(228, 468),
                Size = new Size(88, 28)
            };
            _btnOk.Click += (s, e) => ApplyResultAndClose();

            _btnCancel = new Button
            {
                Text = "Hủy",
                DialogResult = DialogResult.Cancel,
                Location = new Point(322, 468),
                Size = new Size(82, 28)
            };

            Controls.Add(lblMode);
            Controls.Add(_radioAllFiles);
            Controls.Add(_radioDocumentsOnly);
            Controls.Add(lblExt);
            Controls.Add(_extensionsList);
            Controls.Add(lblFolders);
            Controls.Add(_excludedFoldersList);
            Controls.Add(lblNewFolder);
            Controls.Add(_txtNewFolder);
            Controls.Add(_btnAddFolder);
            Controls.Add(_btnRemoveFolder);
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }

        // Đóng gói lựa chọn hiện tại trên form thành một AppSettings mới, gán vào
        // ResultSettings để Form1 đọc lại ngay sau khi ShowDialog() trả về DialogResult.OK.
        private void ApplyResultAndClose()
        {
            var settings = new AppSettings
            {
                Mode = _radioDocumentsOnly.Checked ? ScanMode.DocumentsOnly : ScanMode.AllFiles,
                AllowedExtensions = _extensionsList.CheckedItems.Cast<string>().ToList(),
                ExcludedFolderNames = _excludedFoldersList.Items.Cast<string>().ToList()
            };

            ResultSettings = settings;
            DialogResult = DialogResult.OK;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "SettingsForm";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);

        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {

        }
    }
}