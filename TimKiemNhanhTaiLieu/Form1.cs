using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VBFileIO = Microsoft.VisualBasic.FileIO;

namespace TimKiemNhanhTaiLieu
{
    public partial class Form1 : Sunny.UI.UIForm
    {
        #region Fields

        // Toàn bộ file đã quét/lập chỉ mục. Truy cập qua _indexLock.
        private List<FileInfo> _fileIndex = new List<FileInfo>();
        private readonly object _indexLock = new object();

        // Chỉ mục Lucene lưu bền vững trên đĩa, lần mở sau chỉ cập nhật gia tăng.
        private readonly LuceneSearchService _searchService = new LuceneSearchService(GetIndexStoragePath());

        private AppSettings _appSettings;

        // --- Tìm kiếm ---
        private System.Windows.Forms.Timer _searchTimer;   // debounce khi gõ
        private string _pendingKeyword = string.Empty;     // từ khóa chờ tìm
        private string _currentKeyword = string.Empty;     // từ khóa (chữ thường) đang tô nổi bật
        private int _searchRequestId = 0;                  // đánh số phiên tìm, bỏ kết quả lỗi thời
        private bool _isScanning = false;                  // true khi đang quét/lập chỉ mục

        // --- Sắp xếp (áp dụng lên TOÀN BỘ kết quả, không chỉ trang đang xem) ---
        // Cột lưới: 2 = Tên, 3 = Vị trí, 4 = Kích thước, 5 = Ngày sửa (đúng thứ tự dataFile.Rows.Add)
        private int _sortColumn = -1;          // -1 = chưa sắp xếp (giữ thứ tự quét)
        private bool _sortAscending = true;
        private int _sortVersion = 0;          // tăng mỗi khi người dùng đổi kiểu sắp xếp

        // Tiêu đề gốc của từng cột (để thêm/bớt chú thích kiểu sắp xếp) và tooltip gợi ý khi rê chuột
        private readonly Dictionary<int, string> _headerBaseText = new Dictionary<int, string>();
        private readonly ToolTip _headerTip = new ToolTip();

        // So sánh tên "tự nhiên" như Windows Explorer: file2 < file10
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string x, string y);

        // --- Phân trang ---
        private const int PageSize = 50;
        private List<FileInfo> _currentResultList = new List<FileInfo>(); // danh sách đầy đủ đang hiển thị
        private int _currentPage = 0;                                     // đánh số từ 0

        // --- Cache ---
        private static readonly Dictionary<string, Image> _iconCache =
            new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase); // icon theo đuôi file

        // --- Control tạo bằng code (không cần sửa Designer) ---
        private Panel overlayPanel, overlayInnerPanel, pagingPanel;
        private Label overlayMessageLabel, overlayCountLabel, lblPageInfo;
        private ProgressBar overlayProgress;
        private Button overlayCancelButton, btnPrevPage, btnNextPage, btnSettings;

        private CancellationTokenSource _scanCts;

        // --- Loading khi tìm kiếm ---
        private const int SearchLoadingDelayMs = 200;      // tìm nhanh hơn mức này thì không hiện loading (tránh nháy)
        private Panel searchLoadingPanel;
        private Label searchLoadingLabel;
        private ProgressBar searchLoadingProgress;
        private System.Windows.Forms.Timer _searchLoadingTimer;

        #endregion

        #region Khởi tạo

        private static string GetIndexStoragePath()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, "TimKiemNhanhTaiLieu", "SearchIndex");
        }

        public Form1()
        {
            InitializeComponent();

            // Phải đọc TRƯỚC lần quét đầu tiên (FileForm_Load).
            _appSettings = AppSettings.Load();

            txtPath.Text = @"C:\Users\anh21\OneDrive\Desktop\2.Kien truc may tinh va hop ngu";
            cbSearchMode.SelectedIndex = 0; // mặc định "Tên file"

            BuildOverlay();
            BuildSearchLoading();
            BuildPaging();
            BuildSettingsButton();
            WireSearchEvents();

            FormClosed += (s, e) =>
            {
                _scanCts?.Cancel();
                _scanCts?.Dispose();
                _searchLoadingTimer?.Dispose();
                _searchService?.Dispose();
            };
        }

        private void BuildOverlay()
        {
            overlayPanel = new Panel
            {
                BackColor = Color.FromArgb(220, Color.White),
                BorderStyle = BorderStyle.None,
                Visible = false,
                Anchor = dataFile.Anchor,
                Location = dataFile.Location,
                Size = dataFile.Size
            };

            overlayInnerPanel = new Panel
            {
                Size = new Size(360, 150),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.None
            };

            overlayMessageLabel = new Label
            {
                Text = "Đang quét và cập nhật chỉ mục… Vui lòng chờ.",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 32
            };

            overlayCountLabel = new Label
            {
                Text = "Đã quét: 0 file",
                AutoSize = false,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, overlayMessageLabel.Bottom + 4),
                Size = new Size(overlayInnerPanel.Width, 22)
            };

            overlayProgress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Size = new Size(200, 18),
                Anchor = AnchorStyles.None
            };

            overlayCancelButton = new Button
            {
                Text = "✖  Dừng quét",
                Size = new Size(120, 38),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(220, 53, 69),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                TabStop = false
            };
            overlayCancelButton.FlatAppearance.BorderSize = 0;
            overlayCancelButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);
            overlayCancelButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(176, 30, 43);
            overlayCancelButton.Click += (s, e) =>
            {
                overlayCancelButton.Enabled = false; // tránh bấm nhiều lần
                overlayMessageLabel.Text = "Đang hủy…";
                try { _scanCts?.Cancel(); } catch { }
            };

            overlayInnerPanel.Controls.AddRange(new Control[]
            {
                overlayMessageLabel, overlayCountLabel, overlayProgress, overlayCancelButton
            });
            overlayPanel.Controls.Add(overlayInnerPanel);
            Controls.Add(overlayPanel);
            overlayPanel.BringToFront();

            UpdateOverlayLayout();

            Resize += (s, e) => UpdateOverlayLayout();
            dataFile.SizeChanged += (s, e) => UpdateOverlayLayout();
            dataFile.LocationChanged += (s, e) => UpdateOverlayLayout();
        }

        // Lớp phủ nhẹ "Đang tìm kiếm…" trên lưới. Chỉ hiện nếu tìm lâu hơn SearchLoadingDelayMs.
        private void BuildSearchLoading()
        {
            searchLoadingPanel = new Panel
            {
                BackColor = Color.FromArgb(200, Color.White),
                Visible = false,
                Anchor = dataFile.Anchor,
                Location = dataFile.Location,
                Size = dataFile.Size
            };

            searchLoadingLabel = new Label
            {
                Text = "Đang tìm kiếm… Vui lòng chờ trong giây lát.",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = false,
                Size = new Size(360, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };

            searchLoadingProgress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Size = new Size(200, 16)
            };

            searchLoadingPanel.Controls.Add(searchLoadingLabel);
            searchLoadingPanel.Controls.Add(searchLoadingProgress);
            Controls.Add(searchLoadingPanel);

            _searchLoadingTimer = new System.Windows.Forms.Timer { Interval = SearchLoadingDelayMs };
            _searchLoadingTimer.Tick += (s, e) =>
            {
                _searchLoadingTimer.Stop();
                if (_isScanning) return;
                UpdateOverlayLayout();
                searchLoadingPanel.Visible = true;
                searchLoadingPanel.BringToFront();
                lblResult.Text = "Đang tìm kiếm…";
            };

            // Lớp phủ này chặn chuột vào lưới, không chặn ô nhập nên người dùng vẫn gõ tiếp được.
        }

        private void ShowSearchLoading()
        {
            _searchLoadingTimer.Stop();
            _searchLoadingTimer.Start(); // chỉ hiện nếu tìm vẫn chưa xong sau SearchLoadingDelayMs
        }

        private void HideSearchLoading()
        {
            _searchLoadingTimer.Stop();
            searchLoadingPanel.Visible = false;
        }

        private void BuildPaging()
        {
            pagingPanel = new Panel
            {
                Size = new Size(300, 28),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            pagingPanel.Location = new Point(ClientSize.Width - 60 - pagingPanel.Width, 431);

            btnPrevPage = new Button
            {
                Text = "◀ Trước",
                Size = new Size(80, 26),
                Location = new Point(0, 0),
                Cursor = Cursors.Hand
            };
            btnPrevPage.Click += (s, e) => { _currentPage--; RenderCurrentPage(); };

            lblPageInfo = new Label
            {
                Text = "Trang 1/1",
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(130, 26),
                Location = new Point(btnPrevPage.Right + 5, 0),
                Font = new Font("Segoe UI", 9F)
            };

            btnNextPage = new Button
            {
                Text = "Tiếp ▶",
                Size = new Size(80, 26),
                Location = new Point(lblPageInfo.Right + 5, 0),
                Cursor = Cursors.Hand
            };
            btnNextPage.Click += (s, e) => { _currentPage++; RenderCurrentPage(); };

            pagingPanel.Controls.AddRange(new Control[] { btnPrevPage, lblPageInfo, btnNextPage });
            Controls.Add(pagingPanel);
            pagingPanel.BringToFront();
        }

        private void BuildSettingsButton()
        {
            btnSettings = new Button
            {
                Text = "⚙Cài đặt",
                Size = new Size(125, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnSettings.Location = new Point(ClientSize.Width - 16 - btnSettings.Width, 6);
            btnSettings.Click += async (s, e) => await OpenSettingsAsync();

            Controls.Add(btnSettings);
            btnSettings.BringToFront();
        }

        private void WireSearchEvents()
        {
            _searchTimer = new System.Windows.Forms.Timer { Interval = 600 };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                PerformSearchFromPending();
            };

            // Nút "Tìm kiếm": tìm ngay, không chờ debounce
            btnSearch.Click += (s, e) =>
            {
                _pendingKeyword = txtSearch.Text ?? string.Empty;
                _searchTimer.Stop();
                PerformSearchFromPending();
            };

            // Đổi chế độ tìm: tìm lại ngay với từ khóa hiện tại
            cbSearchMode.SelectedIndexChanged += (s, e) =>
            {
                if (_isScanning) return;
                _pendingKeyword = txtSearch.Text ?? string.Empty;
                PerformSearchFromPending();
            };
        }

        private async Task OpenSettingsAsync()
        {
            using (var settingsForm = new SettingsForm(_appSettings))
            {
                if (settingsForm.ShowDialog(this) != DialogResult.OK) return;

                _appSettings = settingsForm.ResultSettings;
                _appSettings.Save();

                // Cấu hình mới chỉ áp dụng cho lần quét kế tiếp.
                DialogResult reScan = MessageBox.Show(
                    "Đã lưu cài đặt. Bạn có muốn quét lại thư mục hiện tại ngay để áp dụng không?",
                    "Cài đặt", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (reScan == DialogResult.Yes)
                    await LoadAndIndexFilesAsync(txtPath.Text);
            }
        }

        #endregion

        #region Overlay / trạng thái quét

        private void UpdateOverlayLayout()
        {
            if (overlayPanel == null || overlayInnerPanel == null || dataFile == null)
                return;

            overlayPanel.Location = dataFile.Location;
            overlayPanel.Size = dataFile.Size;

            overlayInnerPanel.Location = new Point(
                Math.Max(0, (overlayPanel.Width - overlayInnerPanel.Width) / 2),
                Math.Max(0, (overlayPanel.Height - overlayInnerPanel.Height) / 2));

            overlayProgress.Location = new Point(
                Math.Max(0, (overlayInnerPanel.Width - overlayProgress.Width) / 2),
                overlayCountLabel.Bottom + 8);

            overlayCancelButton.Location = new Point(
                Math.Max(0, (overlayInnerPanel.Width - overlayCancelButton.Width) / 2),
                overlayProgress.Bottom + 10);

            if (searchLoadingPanel != null)
            {
                searchLoadingPanel.Location = dataFile.Location;
                searchLoadingPanel.Size = dataFile.Size;

                int cx = searchLoadingPanel.Width / 2;
                int cy = searchLoadingPanel.Height / 2;
                searchLoadingLabel.Location = new Point(Math.Max(0, cx - searchLoadingLabel.Width / 2), Math.Max(0, cy - 30));
                searchLoadingProgress.Location = new Point(Math.Max(0, cx - searchLoadingProgress.Width / 2), searchLoadingLabel.Bottom + 6);
            }
        }

        /// <summary>
        /// Gọi khi BẮT ĐẦU quét: xóa từ khóa, hủy tìm kiếm đang chờ/đang chạy, xóa dữ liệu bảng
        /// và danh sách đã quét cũ, rồi khóa các control không được dùng trong lúc quét.
        /// </summary>
        private void EnterScanState()
        {
            // Đặt cờ TRƯỚC để txtSearch.Clear() không kích hoạt lại tìm kiếm.
            _isScanning = true;

            _searchTimer.Stop();
            HideSearchLoading();
            _searchRequestId++;                  // vô hiệu hóa kết quả tìm kiếm nền đang chạy dở
            _pendingKeyword = string.Empty;
            _currentKeyword = string.Empty;
            txtSearch.Clear();

            lock (_indexLock) { _fileIndex = new List<FileInfo>(); }
            DisplayFilesToGrid(new List<FileInfo>()); // xóa bảng, về trang 1, tắt nút phân trang
            lblResult.Text = string.Empty;

            SetControlsEnabled(false);

            overlayMessageLabel.Text = "Đang quét thư mục… Vui lòng chờ.";
            overlayCountLabel.Text = "Đã quét: 0 file";
            overlayCancelButton.Enabled = true;
            UpdateOverlayLayout();
            overlayPanel.Visible = true;
            overlayPanel.BringToFront();
        }

        /// <summary>Gọi khi quét xong hoặc bị hủy/lỗi: ẩn overlay và mở lại các control.</summary>
        private void ExitScanState()
        {
            overlayPanel.Visible = false;
            SetControlsEnabled(true);
            _isScanning = false;
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnSelect.Enabled = enabled;
            btnSettings.Enabled = enabled;
            txtSearch.Enabled = enabled;
            btnSearch.Enabled = enabled;
            cbSearchMode.Enabled = enabled;
            btn_map.Enabled = enabled;
            btndelete.Enabled = enabled;
            btnOpen.Enabled = enabled;
        }

        #endregion

        #region Quét & lập chỉ mục

        private async void btnSelect_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Chọn thư mục cần mở";
                fbd.SelectedPath = txtPath.Text;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = fbd.SelectedPath;
                    await LoadAndIndexFilesAsync(txtPath.Text);
                }
            }
        }

        private async void FileForm_Load(object sender, EventArgs e)
        {
            dataFile.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataFile.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dataFile.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dataFile.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Style của TỪNG CỘT (đặt trong Designer hoặc do theme Sunny.UI) được ưu tiên hơn
            // DefaultCellStyle của cả lưới, nên phải đặt căn trái trực tiếp cho cột văn bản.
            // Nếu không, ô thường bị căn giữa còn ô tô vàng (CellPainting vẽ từ mép trái) bị căn trái.
            foreach (string name in new[] { "TenTaiLieu", "ViTri" })
            {
                if (dataFile.Columns.Contains(name))
                    dataFile.Columns[name].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            dataFile.RowHeadersVisible = false;
            dataFile.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataFile.MultiSelect = false;
            dataFile.AllowUserToAddRows = false;
            dataFile.ReadOnly = true;

            dataFile.CellPainting += dataFile_CellPainting; // tô vàng từ khóa khớp

            // Tắt sắp xếp tự động của DataGridView (chỉ sắp xếp được 50 dòng của trang và so sánh
            // theo chuỗi nên "1000 KB" < "20 KB"). Thay bằng sắp xếp theo dữ liệu thật trên toàn bộ kết quả.
            foreach (DataGridViewColumn col in dataFile.Columns)
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
            dataFile.ColumnHeaderMouseClick += dataFile_ColumnHeaderMouseClick;

            // Ghi nhớ tiêu đề gốc + gợi ý "bấm để sắp xếp" (con trỏ bàn tay, tooltip) cho các cột sắp xếp được
            foreach (DataGridViewColumn col in dataFile.Columns)
                _headerBaseText[col.Index] = col.HeaderText;
            dataFile.CellMouseEnter += (s2, e2) =>
            {
                if (e2.RowIndex != -1 || e2.ColumnIndex < 2 || e2.ColumnIndex > 5 || _isScanning) return;
                dataFile.Cursor = Cursors.Hand;
                _headerTip.SetToolTip(dataFile, "Bấm để sắp xếp, bấm lần nữa để đảo chiều");
            };
            dataFile.CellMouseLeave += (s2, e2) =>
            {
                if (e2.RowIndex != -1) return;
                dataFile.Cursor = Cursors.Default;
                _headerTip.SetToolTip(dataFile, string.Empty);
            };

            await LoadAndIndexFilesAsync(txtPath.Text);
        }

        private async Task LoadAndIndexFilesAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Thư mục hoặc ổ đĩa không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Hủy phiên cũ (nếu còn chạy). Giai đoạn quét và giai đoạn lập chỉ mục dùng 2 token
            // riêng: nếu hủy giữa lúc quét, phần đã quét vẫn phải được cập nhật vào chỉ mục.
            _scanCts?.Cancel();
            _scanCts?.Dispose();
            _scanCts = new CancellationTokenSource();
            var scanToken = _scanCts.Token;

            EnterScanState();

            try
            {
                // ---- Giai đoạn 1: quét thư mục ----
                var scanProgress = new Progress<int>(c => overlayCountLabel.Text = $"Đã quét: {c} file");
                var scannedFiles = await Task.Run(() => ScanDirectoryRecursive(folderPath, scanProgress, scanToken));
                bool canceledDuringScan = scanToken.IsCancellationRequested;

                lock (_indexLock) { _fileIndex = scannedFiles; }

                // ---- Giai đoạn 2: cập nhật chỉ mục Lucene (gia tăng) ----
                _scanCts?.Dispose();
                _scanCts = new CancellationTokenSource();
                var indexToken = _scanCts.Token;
                overlayCancelButton.Enabled = true;

                int total = scannedFiles.Count;
                var indexProgress = new Progress<int>(c => overlayCountLabel.Text = $"Đã kiểm tra: {c}/{total} file");

                overlayMessageLabel.Text = canceledDuringScan
                    ? $"Đã dừng quét ở {total} file — đang cập nhật chỉ mục phần này…"
                    : "Đang cập nhật chỉ mục (chỉ đọc lại file mới/đã sửa)… Vui lòng chờ.";
                overlayCountLabel.Text = $"Đã kiểm tra: 0/{total} file";
                UpdateOverlayLayout();

                int processedCount = total;
                try
                {
                    // folderPath bắt buộc để UpdateIndex chỉ dọn mục của thư mục này.
                    processedCount = await Task.Run(() =>
                        _searchService.UpdateIndex(folderPath, scannedFiles, null, indexProgress, indexToken));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi cập nhật chỉ mục nội dung: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                bool canceledDuringIndex = indexToken.IsCancellationRequested;

                // Bị hủy giữa chừng: chỉ giữ phần đã thực sự index để lưới khớp với kết quả tìm.
                if (canceledDuringIndex && processedCount < scannedFiles.Count)
                {
                    lock (_indexLock) { _fileIndex = scannedFiles.Take(processedCount).ToList(); }
                }

                List<FileInfo> snapshot;
                lock (_indexLock) { snapshot = _fileIndex; }
                DisplayFilesToGrid(await Task.Run(() => SortList(snapshot)), true);

                lblResult.Text = (canceledDuringScan || canceledDuringIndex)
                    ? $"Đã hủy. {snapshot.Count} tài liệu đã được cập nhật vào chỉ mục và có thể tìm kiếm."
                    : $"Kết quả: Đã cập nhật chỉ mục cho {snapshot.Count} tài liệu.";
            }
            finally
            {
                ExitScanState(); // luôn mở lại UI, kể cả khi có lỗi bất ngờ
            }
        }

        // Duyệt thư mục bằng Stack (không đệ quy). Khi bị hủy sẽ dừng "êm" (không ném exception)
        // và trả về phần đã quét được; bên gọi kiểm tra token để biết kết quả đầy đủ hay một phần.
        private List<FileInfo> ScanDirectoryRecursive(
            string rootPath,
            IProgress<int> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            var fileList = new List<FileInfo>();
            var stack = new Stack<string>();
            stack.Push(rootPath);

            while (stack.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                string currentDir = stack.Pop();

                try
                {
                    foreach (string file in Directory.GetFiles(currentDir))
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        // Lọc theo whitelist đuôi file (chế độ DocumentsOnly)
                        if (!_appSettings.IsExtensionAllowed(Path.GetExtension(file)))
                            continue;

                        fileList.Add(new FileInfo(file));

                        if (fileList.Count % 20 == 0) // báo tiến độ thưa để không ảnh hưởng hiệu năng
                            onProgress?.Report(fileList.Count);
                    }
                }
                catch (UnauthorizedAccessException) { continue; }
                catch (DirectoryNotFoundException) { continue; }

                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    foreach (string dir in Directory.GetDirectories(currentDir))
                    {
                        // Bỏ qua thư mục loại trừ (node_modules, .git, Windows, AppData...)
                        if (_appSettings.IsFolderExcluded(Path.GetFileName(dir)))
                            continue;

                        stack.Push(dir);
                    }
                }
                catch (UnauthorizedAccessException) { continue; }
                catch (DirectoryNotFoundException) { continue; }
            }

            onProgress?.Report(fileList.Count);
            return fileList;
        }

        #endregion

        #region Hiển thị lưới & phân trang

        // Lấy icon theo đuôi file, cache để không đọc shell/đĩa lặp lại cho từng dòng.
        private Image GetFileIcon(string filePath)
        {
            string ext = Path.GetExtension(filePath) ?? string.Empty;

            if (_iconCache.TryGetValue(ext, out var cached))
                return cached;

            Image icon;
            try
            {
                if (File.Exists(filePath))
                {
                    using (Icon ico = Icon.ExtractAssociatedIcon(filePath))
                        icon = ico != null ? ico.ToBitmap() : SystemIcons.Application.ToBitmap();
                }
                else
                {
                    icon = SystemIcons.Application.ToBitmap();
                }
            }
            catch
            {
                icon = SystemIcons.Application.ToBitmap();
            }

            _iconCache[ext] = icon;
            return icon;
        }

        // Nhận danh sách đầy đủ cần hiển thị, về trang 1 và vẽ trang đầu.
        // alreadySorted = true khi danh sách đã được sắp xếp sẵn ở luồng nền.
        private void DisplayFilesToGrid(List<FileInfo> files, bool alreadySorted = false)
        {
            _currentResultList = alreadySorted ? (files ?? new List<FileInfo>()) : SortList(files);
            _currentPage = 0;
            RenderCurrentPage();
            UpdateSortGlyphs();
        }

        // FileInfo.Length ném FileNotFoundException nếu file không còn tồn tại (bị xóa/di chuyển sau khi
        // quét, ổ đĩa tháo ra, đường dẫn quá dài...). Trả về -1 thay vì làm sập ứng dụng.
        private static long SafeLength(FileInfo fi)
        {
            try { return fi.Length; }
            catch { return -1; }
        }

        // Trả về bản sao đã sắp xếp theo kiểu hiện tại (không đụng tới list gốc); an toàn gọi ở luồng nền.
        private List<FileInfo> SortList(List<FileInfo> files)
        {
            if (files == null) return new List<FileInfo>();

            int col = _sortColumn;
            bool asc = _sortAscending;

            Comparison<FileInfo> primary;
            switch (col)
            {
                case 2: primary = (a, b) => StrCmpLogicalW(a.Name, b.Name); break;
                case 3: primary = (a, b) => StrCmpLogicalW(a.DirectoryName ?? string.Empty, b.DirectoryName ?? string.Empty); break;
                case 4:
                    // Đọc kích thước MỘT LẦN cho mỗi file (an toàn nếu file đã bị xóa/di chuyển)
                    var sizes = new Dictionary<FileInfo, long>(files.Count);
                    foreach (var f in files) sizes[f] = SafeLength(f);
                    primary = (a, b) => sizes[a].CompareTo(sizes[b]);
                    break;
                case 5: primary = (a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime); break;
                default: return files; // chưa chọn cột sắp xếp
            }

            var sorted = new List<FileInfo>(files);
            sorted.Sort((a, b) =>
            {
                int r = primary(a, b);
                if (r != 0) return asc ? r : -r;
                return StrCmpLogicalW(a.FullName, b.FullName); // bằng nhau thì xếp theo đường dẫn cho ổn định
            });
            return sorted;
        }

        // Hiển thị kiểu sắp xếp bằng CHỮ dễ hiểu ngay trên tiêu đề cột (vd "Ngày sửa (mới → cũ)")
        // và tô nổi bật cột đang sắp xếp, thay cho mũi tên ▲▼ mặc định.
        private void UpdateSortGlyphs()
        {
            foreach (DataGridViewColumn c in dataFile.Columns)
            {
                c.HeaderCell.SortGlyphDirection = SortOrder.None; // tắt mũi tên mặc định

                string baseText;
                if (!_headerBaseText.TryGetValue(c.Index, out baseText)) continue;

                if (c.Index != _sortColumn)
                {
                    c.HeaderText = baseText;
                    c.HeaderCell.Style.BackColor = Color.Empty;
                    c.HeaderCell.Style.ForeColor = Color.Empty;
                    continue;
                }

                c.HeaderText = $"{baseText} ({GetSortHint(c.Index, _sortAscending)})";
                c.HeaderCell.Style.BackColor = Color.FromArgb(222, 236, 252); // xanh nhạt
                c.HeaderCell.Style.ForeColor = Color.FromArgb(25, 90, 170);
            }
        }

        private static string GetSortHint(int column, bool ascending)
        {
            switch (column)
            {
                case 4: return ascending ? "nhỏ → lớn" : "lớn → nhỏ";
                case 5: return ascending ? "cũ → mới" : "mới → cũ";
                default: return ascending ? "A → Z" : "Z → A";
            }
        }

        private async void dataFile_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (_isScanning || e.ColumnIndex < 2 || e.ColumnIndex > 5) return; // bỏ qua cột STT/Icon

            if (_sortColumn == e.ColumnIndex)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn = e.ColumnIndex;
                _sortAscending = e.ColumnIndex < 4; // Tên/Vị trí: A→Z; Kích thước/Ngày: lớn nhất/mới nhất trước
            }
            _sortVersion++;

            int requestId = _searchRequestId;
            var source = _currentResultList;

            ShowSearchLoading();
            try
            {
                var sorted = await Task.Run(() => SortList(source));
                if (requestId != _searchRequestId) return; // vừa có tìm kiếm mới, nó sẽ tự áp dụng sắp xếp
                DisplayFilesToGrid(sorted, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể sắp xếp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideSearchLoading();
            }
        }

        // Vẽ đúng 1 trang (PageSize dòng). _currentPage được chặn về khoảng hợp lệ tại đây,
        // nên nơi gọi chỉ cần tăng/giảm _currentPage.
        private void RenderCurrentPage()
        {
            dataFile.Rows.Clear();

            int totalItems = _currentResultList.Count;
            int totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
            _currentPage = Math.Max(0, Math.Min(_currentPage, totalPages - 1));

            int startIndex = _currentPage * PageSize;
            int stt = startIndex + 1; // STT liên tục xuyên suốt các trang

            foreach (FileInfo fi in _currentResultList.Skip(startIndex).Take(PageSize))
            {
                long len = SafeLength(fi);
                dataFile.Rows.Add(
                    stt++,
                    GetFileIcon(fi.FullName),
                    fi.Name,
                    fi.DirectoryName,
                    len >= 0 ? (len / 1024) + " KB" : "—",
                    fi.LastWriteTime.ToString("dd/MM/yyyy HH:mm"));
            }

            lblPageInfo.Text = $"Trang {_currentPage + 1}/{totalPages} (Tổng: {totalItems})";
            btnPrevPage.Enabled = _currentPage > 0;
            btnNextPage.Enabled = _currentPage < totalPages - 1;
        }

        #endregion

        #region Tìm kiếm

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isScanning) return; // đang quét: bỏ qua (kể cả khi code tự xóa từ khóa)

            _pendingKeyword = txtSearch.Text ?? string.Empty;
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        // Tìm thực sự dựa trên _pendingKeyword. Chế độ theo cbSearchMode:
        // 0 = Tên file, 1 = Nội dung file, 2 = Cả hai.
        private void PerformSearchFromPending()
        {
            if (_isScanning) return;

            string keyword = _pendingKeyword.Trim();
            SearchMode mode = (SearchMode)Math.Max(0, cbSearchMode.SelectedIndex);

            // Kết quả của phiên cũ (id nhỏ hơn) sẽ bị bỏ qua để không ghi đè kết quả mới hơn.
            int requestId = ++_searchRequestId;

            if (string.IsNullOrEmpty(keyword))
            {
                ShowSearchLoading();
                Task.Run(() =>
                {
                    List<FileInfo> all;
                    lock (_indexLock) { all = _fileIndex; }
                    int sortVer = _sortVersion;
                    var sorted = SortList(all);

                    BeginInvoke((Action)(() =>
                    {
                        if (requestId != _searchRequestId) return;
                        try
                        {
                            _currentKeyword = string.Empty;
                            DisplayFilesToGrid(sorted, sortVer == _sortVersion);
                            lblResult.Text = $"Kết quả: Đã lập chỉ mục {all.Count} tài liệu.";
                            dataFile.Invalidate();
                        }
                        finally { HideSearchLoading(); }
                    }));
                });
                return;
            }

            ShowSearchLoading();

            Task.Run(() =>
            {
                HashSet<string> matchedPaths;
                try
                {
                    matchedPaths = _searchService.Search(keyword, mode);
                }
                catch (Exception ex)
                {
                    matchedPaths = new HashSet<string>();
                    BeginInvoke((Action)(() =>
                        MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                }

                List<FileInfo> matched;
                lock (_indexLock)
                {
                    matched = _fileIndex.Where(fi => matchedPaths.Contains(fi.FullName)).ToList();
                }
                int sortVer = _sortVersion;
                matched = SortList(matched); // sắp xếp ở luồng nền để UI không bị đứng

                BeginInvoke((Action)(() =>
                {
                    // Phiên cũ: không ẩn loading vì phiên mới hơn vẫn đang chạy.
                    if (requestId != _searchRequestId) return;

                    try
                    {
                        // Tìm theo nội dung file: không tô nổi bật ở cột tên (tên file không nhất thiết chứa từ khóa)
                        _currentKeyword = mode == SearchMode.Content ? string.Empty : keyword.ToLower();
                        DisplayFilesToGrid(matched, sortVer == _sortVersion);

                        string modeText = mode == SearchMode.FileName ? "tên file"
                            : mode == SearchMode.Content ? "nội dung file"
                            : "tên và nội dung";
                        lblResult.Text = $"Tìm thấy: {matched.Count} tài liệu khớp (tìm theo {modeText}).";
                        dataFile.Invalidate();
                    }
                    finally
                    {
                        HideSearchLoading();
                    }
                }));
            });
        }

        // Vẽ cột "Tên tài liệu" và tô nổi bật từ khóa. CHỈ áp dụng cho cột tên (không tô cột Vị trí).
        // Mọi ô của cột tên đều do hàm này vẽ (kể cả khi không có từ khóa) để vị trí chữ luôn đồng nhất,
        // không bị lệch giữa ô tô nổi bật và ô thường.
        private static readonly TextFormatFlags MeasureFlags =
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

        private void dataFile_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex != dataFile.Columns["TenTaiLieu"].Index)
                    return;

                string text = e.FormattedValue?.ToString() ?? string.Empty;
                var font = e.CellStyle.Font ?? dataFile.Font;
                bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
                Color foreColor = selected ? e.CellStyle.SelectionForeColor : e.CellStyle.ForeColor;

                e.Handled = true;
                e.PaintBackground(e.ClipBounds, selected); // nền (gồm màu chọn dòng) + viền ô

                var g = e.Graphics;
                int left = e.CellBounds.X + 6;
                Rectangle textArea = new Rectangle(left, e.CellBounds.Y, Math.Max(1, e.CellBounds.Right - left - 4), e.CellBounds.Height);
                const TextFormatFlags drawFlags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix |
                                                  TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.Left;

                var ranges = FindHighlightRanges(text, _currentKeyword);
                if (ranges.Count == 0)
                {
                    TextRenderer.DrawText(g, text, font, textArea, foreColor, drawFlags | TextFormatFlags.EndEllipsis);
                    return;
                }

                // Tọa độ x của ký tự thứ n, đo từ đầu chuỗi (đo cả đoạn thay vì cộng dồn từng mảnh
                // nên không bị lệch khoảng cách). Thêm "|" để không mất độ rộng khoảng trắng cuối.
                Size huge = new Size(int.MaxValue, int.MaxValue);
                int barWidth = TextRenderer.MeasureText(g, "|", font, huge, MeasureFlags).Width;
                Func<int, int> posOf = n => n <= 0 ? 0
                    : TextRenderer.MeasureText(g, text.Substring(0, n) + "|", font, huge, MeasureFlags).Width - barWidth;

                int textHeight = TextRenderer.MeasureText(g, "Ag", font, huge, MeasureFlags).Height;
                int boxHeight = Math.Min(e.CellBounds.Height - 4, textHeight + 4);
                int boxY = e.CellBounds.Y + (e.CellBounds.Height - boxHeight) / 2;

                var oldClip = g.Clip;
                var oldSmoothing = g.SmoothingMode;
                g.SetClip(Rectangle.Intersect(e.CellBounds, e.ClipBounds));
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                try
                {
                    // 1) Nền highlight bo góc nhẹ
                    using (var brush = new SolidBrush(Color.FromArgb(255, 232, 110)))
                    {
                        foreach (var r in ranges)
                        {
                            int x1 = left + posOf(r.Item1);
                            int x2 = left + posOf(r.Item1 + r.Item2);
                            var box = new Rectangle(x1 - 1, boxY, Math.Max(2, x2 - x1 + 2), boxHeight);
                            using (var path = RoundedRect(box, 3))
                                g.FillPath(brush, path);
                        }
                    }

                    // 2) Chữ: đoạn thường theo màu mặc định, đoạn khớp màu đen cho dễ đọc trên nền vàng
                    int cursor = 0;
                    foreach (var r in ranges)
                    {
                        DrawSegment(text.Substring(cursor, r.Item1 - cursor), cursor);
                        DrawSegment(text.Substring(r.Item1, r.Item2), r.Item1, Color.Black);
                        cursor = r.Item1 + r.Item2;
                    }
                    DrawSegment(text.Substring(cursor), cursor);

                    void DrawSegment(string seg, int startIndex, Color? color = null)
                    {
                        if (seg.Length == 0) return;
                        int x = left + posOf(startIndex);
                        var area = new Rectangle(x, e.CellBounds.Y, Math.Max(1, e.CellBounds.Right - x), e.CellBounds.Height);
                        TextRenderer.DrawText(g, seg, font, area, color ?? foreColor, drawFlags);
                    }
                }
                finally
                {
                    g.SmoothingMode = oldSmoothing;
                    g.Clip = oldClip;
                }
            }
            catch
            {
                e.Handled = false; // lỗi khi vẽ: dùng hành vi mặc định
            }
        }

        // Tìm các đoạn (vị trí, độ dài) khớp từng từ trong từ khóa, không phân biệt hoa thường.
        // Từ khóa nhiều từ ("kiến trúc") tô riêng từng từ; các đoạn chồng nhau được gộp lại.
        private static List<Tuple<int, int>> FindHighlightRanges(string text, string keyword)
        {
            var result = new List<Tuple<int, int>>();
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(keyword)) return result;

            var spans = new List<int[]>();
            var terms = keyword.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Distinct();
            foreach (string term in terms)
            {
                int pos = 0;
                while ((pos = text.IndexOf(term, pos, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    spans.Add(new[] { pos, pos + term.Length });
                    pos += term.Length;
                }
            }

            spans.Sort((x, y) => x[0].CompareTo(y[0]));
            int curStart = -1, curEnd = -1;
            foreach (var sp in spans)
            {
                if (curStart < 0) { curStart = sp[0]; curEnd = sp[1]; }
                else if (sp[0] <= curEnd) curEnd = Math.Max(curEnd, sp[1]);
                else { result.Add(Tuple.Create(curStart, curEnd - curStart)); curStart = sp[0]; curEnd = sp[1]; }
            }
            if (curStart >= 0) result.Add(Tuple.Create(curStart, curEnd - curStart));
            return result;
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion

        #region Thao tác trên file đang chọn

        // Lấy đường dẫn đầy đủ của dòng đang chọn; hiện thông báo và trả false nếu không hợp lệ.
        private bool TryGetSelectedFilePath(out string filePath)
        {
            filePath = null;

            if (dataFile.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một tài liệu trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            var folderCell = dataFile.CurrentRow.Cells["ViTri"].Value;
            var fileCell = dataFile.CurrentRow.Cells["TenTaiLieu"].Value;
            if (folderCell == null || fileCell == null)
            {
                MessageBox.Show("Không lấy được thông tin file!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            filePath = Path.Combine(folderCell.ToString(), fileCell.ToString());
            if (!File.Exists(filePath))
            {
                MessageBox.Show("File không tồn tại hoặc đã bị xóa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void btn_map_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedFilePath(out string filePath)) return;

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở Explorer: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedFilePath(out string filePath)) return;

            try
            {
                System.Diagnostics.Process.Start(filePath); // mở bằng ứng dụng mặc định
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btndelete_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedFilePath(out string filePath)) return;

            string fullPath = Path.GetFullPath(filePath).ToLower();
            if (fullPath.StartsWith(@"c:\windows") ||
                fullPath.StartsWith(@"c:\program files") || // bao gồm cả "program files (x86)"
                fullPath.Contains(@"\appdata\"))
            {
                MessageBox.Show("Không được phép xóa file hệ thống hoặc file trong thư mục quan trọng!",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn đưa tài liệu này vào thùng rác?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                VBFileIO.FileSystem.DeleteFile(filePath,
                    VBFileIO.UIOption.OnlyErrorDialogs,
                    VBFileIO.RecycleOption.SendToRecycleBin);

                lock (_indexLock)
                {
                    var item = _fileIndex.FirstOrDefault(fi => fi.FullName == filePath);
                    if (item != null) _fileIndex.Remove(item);
                }

                try { _searchService.RemoveDocument(filePath); } catch { }

                // _currentResultList có thể là danh sách kết quả tìm riêng nên phải xóa tường minh.
                var inView = _currentResultList.FirstOrDefault(fi => fi.FullName == filePath);
                if (inView != null) _currentResultList.Remove(inView);

                RenderCurrentPage(); // tự lùi trang nếu trang hiện tại hết dữ liệu
                lblResult.Text = "Kết quả: " + _currentResultList.Count + " tài liệu được tìm thấy.";

                MessageBox.Show("Đã đưa vào thùng rác: " + Path.GetFileName(filePath),
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đưa file vào thùng rác: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn thoát chương trình?",
                "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                Application.Exit();
        }

        // Giữ lại vì có thể đang được gắn trong Form1.Designer.cs.
        private void lblResult_Click(object sender, EventArgs e) { }

        #endregion
    }
}