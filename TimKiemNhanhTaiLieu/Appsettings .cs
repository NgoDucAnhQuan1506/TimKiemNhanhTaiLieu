using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TimKiemNhanhTaiLieu
{
    /// <summary>
    /// Chế độ quét file:
    ///   AllFiles      = quét TẤT CẢ file trong thư mục/ổ đĩa, không lọc theo đuôi.
    ///   DocumentsOnly = chỉ quét file có đuôi nằm trong AllowedExtensions bên dưới.
    /// </summary>
    public enum ScanMode
    {
        AllFiles = 0,
        DocumentsOnly = 1
    }

    /// <summary>
    /// Cấu hình của ứng dụng, được LƯU BỀN VỮNG trên đĩa dưới dạng text đơn giản (Key=Value)
    /// tại %LOCALAPPDATA%\TimKiemNhanhTaiLieu\settings.txt (cùng thư mục gốc với SearchIndex),
    /// để lựa chọn của người dùng (chế độ quét, whitelist đuôi file, thư mục loại trừ...)
    /// không bị mất khi tắt ứng dụng. Xem ghi chú ở Load()/Save() về lý do KHÔNG dùng JSON.
    ///
    /// Mặc định (khi chưa từng lưu lần nào): Mode = DocumentsOnly, với whitelist khớp đúng
    /// các định dạng mà LuceneSearchService.ExtractContent() có hỗ trợ trích xuất nội dung -
    /// vì quét thêm những đuôi không được hỗ trợ trích nội dung (ảnh, exe, dll...) chỉ tốn
    /// thời gian quét/index mà không mang lại lợi ích tìm kiếm nào.
    /// </summary>
    public class AppSettings
    {
        public ScanMode Mode { get; set; } = ScanMode.DocumentsOnly;

        public List<string> AllowedExtensions { get; set; } = new List<string>
        {
            ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt",
            ".rtf", ".html", ".htm", ".json", ".pdf",
            ".txt", ".md", ".csv", ".log"
        };

        // Tên thư mục (chỉ so theo TÊN, không phải đường dẫn đầy đủ, không phân biệt hoa/thường)
        // sẽ bị BỎ QUA hoàn toàn khi quét - áp dụng cho CẢ HAI chế độ (AllFiles lẫn
        // DocumentsOnly), vì những thư mục này gần như chắc chắn không chứa tài liệu cần tìm,
        // duyệt vào chỉ tốn thời gian một cách vô ích.
        public List<string> ExcludedFolderNames { get; set; } = new List<string>
        {
            "node_modules", ".git", "$RECYCLE.BIN", "System Volume Information",
            "Windows", "Program Files", "Program Files (x86)", "AppData"
        };

        // Ký tự phân cách giữa các phần tử trong một danh sách (AllowedExtensions,
        // ExcludedFolderNames) khi ghi ra file text. Chọn ký tự hiếm gặp trong tên đuôi
        // file/thư mục để tránh nhầm lẫn khi tách chuỗi.
        private const char ListSeparator = ';';

        private static string GetSettingsPath()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // Đổi tên file thành .txt (thay vì .json) vì định dạng lưu bên dưới không còn là
            // JSON thật sự nữa - chỉ là các dòng "Key=Value" đơn giản (xem giải thích ở Save()).
            return Path.Combine(basePath, "TimKiemNhanhTaiLieu", "settings.txt");
        }

        /// <summary>
        /// Đọc cấu hình từ đĩa. Nếu chưa có file (lần chạy đầu tiên) hoặc file bị hỏng/không
        /// đọc được, trả về cấu hình MẶC ĐỊNH (không ném lỗi ra ngoài), để app luôn khởi động
        /// được kể cả khi file settings có vấn đề.
        ///
        /// LƯU Ý: hàm này CỐ TÌNH không dùng System.Text.Json/Newtonsoft.Json. Trên một số máy,
        /// System.Text.Json yêu cầu assembly System.ValueTuple (hoặc gói NuGet phụ trợ khác)
        /// được nạp đúng phiên bản qua binding redirect trong App.config - nếu thiếu, ứng dụng sẽ
        /// văng lỗi "Could not load file or assembly 'System.ValueTuple...'" ngay khi khởi động,
        /// trước cả khi vào được màn hình chính. Dùng định dạng "Key=Value" tự viết dưới đây để
        /// tránh phụ thuộc bất kỳ assembly nào ngoài .NET Framework có sẵn.
        /// </summary>
        public static AppSettings Load()
        {
            var settings = new AppSettings();
            try
            {
                string path = GetSettingsPath();
                if (!File.Exists(path))
                    return settings;

                foreach (string rawLine in File.ReadAllLines(path))
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;

                    int eq = line.IndexOf('=');
                    if (eq < 0) continue;

                    string key = line.Substring(0, eq).Trim();
                    string value = line.Substring(eq + 1).Trim();

                    switch (key)
                    {
                        case nameof(Mode):
                            if (Enum.TryParse<ScanMode>(value, true, out var mode))
                                settings.Mode = mode;
                            break;

                        case nameof(AllowedExtensions):
                            settings.AllowedExtensions = SplitList(value);
                            break;

                        case nameof(ExcludedFolderNames):
                            settings.ExcludedFolderNames = SplitList(value);
                            break;
                    }
                }
            }
            catch
            {
                return new AppSettings();
            }

            return settings;
        }

        /// <summary>
        /// Ghi cấu hình hiện tại xuống đĩa dưới dạng các dòng "Key=Value" đơn giản, tự tạo thư
        /// mục chứa nếu chưa có. Xem ghi chú ở Load() về lý do không dùng thư viện JSON.
        /// </summary>
        public void Save()
        {
            string path = GetSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var sb = new StringBuilder();
            sb.AppendLine("# File cấu hình của TimKiemNhanhTaiLieu - có thể chỉnh sửa trực tiếp nếu cần.");
            sb.AppendLine($"{nameof(Mode)}={Mode}");
            sb.AppendLine($"{nameof(AllowedExtensions)}={JoinList(AllowedExtensions)}");
            sb.AppendLine($"{nameof(ExcludedFolderNames)}={JoinList(ExcludedFolderNames)}");

            File.WriteAllText(path, sb.ToString());
        }

        private static string JoinList(List<string> items)
        {
            return string.Join(ListSeparator.ToString(), items ?? new List<string>());
        }

        private static List<string> SplitList(string value)
        {
            if (string.IsNullOrEmpty(value)) return new List<string>();
            return value
                .Split(new[] { ListSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToList();
        }

        /// <summary>
        /// Kiểm tra một tên thư mục (không phải đường dẫn đầy đủ) có nằm trong danh sách loại
        /// trừ hay không, dùng để quyết định có duyệt tiếp vào thư mục đó khi quét hay không.
        /// </summary>
        public bool IsFolderExcluded(string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) return false;
            return ExcludedFolderNames.Any(x => string.Equals(x, folderName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Kiểm tra một đuôi file (ví dụ ".pdf", có dấu chấm ở đầu) có được phép quét hay không.
        /// Ở chế độ AllFiles, MỌI đuôi đều được chấp nhận; ở chế độ DocumentsOnly, chỉ những
        /// đuôi có trong AllowedExtensions mới được chấp nhận.
        /// </summary>
        public bool IsExtensionAllowed(string extension)
        {
            if (Mode == ScanMode.AllFiles) return true;
            if (string.IsNullOrEmpty(extension)) return false;
            return AllowedExtensions.Any(x => string.Equals(x, extension, StringComparison.OrdinalIgnoreCase));
        }
    }
}