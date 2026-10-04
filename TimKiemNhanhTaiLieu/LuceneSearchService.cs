using DocumentFormat.OpenXml.Packaging;
using ExcelDataReader;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util; // CharArraySet (tắt stop word)
using Lucene.Net.Analysis.Miscellaneous; // PerFieldAnalyzerWrapper (analyzer riêng cho field content)
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SautinSoft.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UglyToad.PdfPig;
// KHÔNG "using DocumentFormat.OpenXml.Wordprocessing;" trực tiếp vì namespace này cũng có
// type tên Document/Query trùng tên với Lucene.Net.Documents.Document và Lucene.Net.Search.Query
// -> gây lỗi "ambiguous reference". Dùng alias riêng cho namespace này thay thế.
using WordXml = DocumentFormat.OpenXml.Wordprocessing;

namespace TimKiemNhanhTaiLieu
{
    /// <summary>
    /// Chế độ tìm kiếm tương ứng với cbSearchMode.SelectedIndex trong Form1:
    /// 0 = Tên file, 1 = Nội dung file, 2 = Cả hai.
    /// </summary>
    public enum SearchMode
    {
        FileName = 0,
        Content = 1,
        Both = 2
    }

    /// <summary>
    /// Dịch vụ lập chỉ mục (indexing) và tìm kiếm tài liệu bằng Lucene.NET.
    ///
    /// THAY ĐỔI QUAN TRỌNG so với bản trước:
    /// - C:\Users\anh21\AppData\Local\TimKiemNhanhTaiLieu
    /// - Chỉ mục giờ được LƯU XUỐNG ĐĨA (FSDirectory) tại một thư mục cố định trong
    ///   %LOCALAPPDATA%, thay vì chỉ nằm trong RAM (RAMDirectory). Nhờ vậy, chỉ mục
    ///   KHÔNG bị mất khi tắt ứng dụng, và lần mở app kế tiếp không cần build lại từ đầu.
    /// - Có thêm UpdateIndex(): thay vì BuildIndex() ném đi toàn bộ index cũ và làm lại,
    ///   UpdateIndex() so sánh với dữ liệu đã lưu (dựa trên đường dẫn file + thời gian
    ///   sửa đổi cuối cùng - LastWriteTimeUtc) để:
    ///     + Bỏ qua (không đọc lại nội dung) những file KHÔNG thay đổi từ lần quét trước.
    ///     + Chỉ trích xuất nội dung (đọc docx/pdf/...) cho file MỚI hoặc ĐÃ SỬA ĐỔI.
    ///     + Xóa khỏi chỉ mục những file không còn tồn tại trong lần quét hiện tại.
    ///   Nhờ vậy, những lần mở app sau chỉ mất thời gian quét thư mục (liệt kê file),
    ///   phần tốn thời gian nhất (đọc nội dung docx/pdf để lập chỉ mục) chỉ chạy lại
    ///   cho phần thay đổi.
    /// - BuildIndex() (rebuild toàn bộ, không giữ lại gì) vẫn được giữ lại phòng khi cần
    ///   "làm mới hoàn toàn" chỉ mục (ví dụ người dùng nghi ngờ chỉ mục bị hỏng).
    ///
    /// TỐI ƯU TỐC ĐỘ KHI ĐỔI THƯ MỤC / Ổ ĐĨA (bổ sung):
    /// - Chỉ mục trên đĩa là DÙNG CHUNG cho mọi thư mục/ổ đĩa từng quét (đây là chủ đích thiết
    ///   kế - đổi qua lại giữa các thư mục không mất index). Nhưng vì vậy, càng dùng lâu chỉ
    ///   mục càng phình to.
    /// - Trước đây, để biết "file nào đã có trong index, ticks bao nhiêu", UpdateIndex() phải
    ///   duyệt (Document(i)) TOÀN BỘ document trong index, kể cả những document thuộc các thư
    ///   mục/ổ đĩa KHÁC không liên quan tới lần quét hiện tại -> đây chính là nguyên nhân gây
    ///   lag khi mở app hoặc đổi thư mục, vì chi phí tỉ lệ với TỔNG số file đã từng index.
    /// - Nay dùng PrefixQuery trên field FIELD_PATH_LOWER (không phân tích, giống StringField)
    ///   để tận dụng term dictionary của Lucene (giống cấu trúc B-Tree): tra cứu và duyệt CHỈ
    ///   những document có "path_lower" bắt đầu bằng đường dẫn thư mục đang quét, không đụng
    ///   tới các document của thư mục khác -> chi phí chỉ còn tỉ lệ với số file THỰC SỰ thuộc
    ///   thư mục hiện tại, không phụ thuộc kích thước tổng của toàn bộ chỉ mục.
    ///
    /// SỬA LỖI TÌM KIẾM NỘI DUNG THEO TIỀN TỐ / CỤM TỪ (bản này):
    /// - Bỏ giới hạn 1024 term khi mở rộng tiền tố ở từ cuối. Trước đây với tiền tố ngắn như "l",
    ///   Lucene chỉ giữ 1024 term và ưu tiên bỏ các term "lớn hơn" theo thứ tự byte (term có dấu
    ///   như "là" thường bị loại) -> "hợp ngữ l" không ra "hợp ngữ là một".
    /// - Chuẩn hóa Unicode về NFC cho cả nội dung lẫn từ khóa (chữ tiếng Việt trích từ PDF/Word
    ///   thường ở dạng tổ hợp, còn bàn phím gõ ra dạng dựng sẵn -> hai term khác nhau dù nhìn giống).
    /// - StandardAnalyzer không còn loại stop word tiếng Anh (the, of, in...) để các từ luôn liền
    ///   kề đúng vị trí khi so khớp cụm từ (SpanNearQuery slop = 0).
    /// - Search() lấy TOÀN BỘ kết quả khớp thay vì chỉ 2000 kết quả đầu.
    /// - Có thêm tham số exactWords ở Search(): true = từ cuối phải khớp nguyên từ, false = khớp tiền tố.
    /// - Có IndexSchemaVersion: khi đổi cách lập chỉ mục, chỉ mục cũ trên đĩa tự bị xóa và
    ///   UpdateIndex sẽ lập lại từ đầu.
    ///
    /// THAY ĐỔI MỚI NHẤT - EDGE NGRAM LÚC INDEX THAY VÌ PREFIXQUERY LÚC TÌM:
    /// - Trước đây, khớp tiền tố ở từ cuối dùng SpanMultiTermQueryWrapper&lt;PrefixQuery&gt; LÚC TÌM,
    ///   nghĩa là MỖI LẦN gõ phím, Lucene phải quét term dictionary và MỞ RỘNG toàn bộ term khớp
    ///   tiền tố (với tiền tố ngắn như "l", có thể là hàng nghìn term), rồi gộp vị trí của tất cả
    ///   -> đây là nguyên nhân chính gây lag khi tìm theo nội dung.
    /// - Nay chuyển chi phí này sang LÚC LẬP CHỈ MỤC (chỉ trả 1 lần cho file mới/đã sửa, nhờ
    ///   UpdateIndex gia tăng): field content được lập chỉ mục bằng ContentPrefixAnalyzer
    ///   (xem EdgeNGramPrefixFilter.cs) - với mỗi từ, ngoài từ đầy đủ, còn sinh sẵn TẤT CẢ tiền
    ///   tố của từ đó (có đánh dấu bằng EdgeNGramPrefixFilter.MarkerChar), xếp chồng cùng vị trí.
    ///   Lúc tìm, tra tiền tố chỉ còn là một TermQuery tra thẳng term dictionary - O(1), không
    ///   còn mở rộng gì cả.
    /// - Field content dùng PerFieldAnalyzerWrapper: field "content" dùng ContentPrefixAnalyzer
    ///   (sinh gram) khi GHI; các field khác (filename, path...) và việc PHÂN TÍCH TỪ KHÓA lúc
    ///   TÌM (AnalyzeTerms) vẫn dùng _analyzer (StandardAnalyzer thường, không sinh gram).
    /// - IndexSchemaVersion tăng lên "3" để tự động xóa và lập lại chỉ mục cũ (không tương thích
    ///   với cách lập chỉ mục mới).
    ///
    /// Cần cài các gói NuGet:
    ///   - Lucene.Net (bản 4.8.0-beta00016 trở lên)
    ///   - Lucene.Net.Analysis.Common
    ///   - Lucene.Net.QueryParser
    ///   - DocumentFormat.OpenXml   (đọc nội dung .docx)
    ///   - PdfPig                  (đọc nội dung .pdf)
    /// </summary>
    public class LuceneSearchService : IDisposable
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        // Phiên bản "cấu trúc" của chỉ mục. TĂNG SỐ NÀY mỗi khi thay đổi cách lập chỉ mục
        // (analyzer, chuẩn hóa văn bản, các field...) để chỉ mục cũ trên đĩa tự bị xóa và làm lại.
        // Tăng lên "3": field content chuyển sang dùng ContentPrefixAnalyzer (EdgeNGram tiền tố)
        // thay vì StandardAnalyzer thường + PrefixQuery lúc tìm.
        private const string IndexSchemaVersion = "3";
        private const string IndexSchemaMarkerFile = "schema.version";

        // Giới hạn dung lượng file khi trích nội dung để tránh treo UI với file quá lớn (mặc định 30MB)
        private const long MaxContentFileSizeBytes = 30L * 1024 * 1024;

        // Directory là lớp cơ sở của cả FSDirectory (lưu trên đĩa) và RAMDirectory (lưu trong RAM).
        // Dùng kiểu cơ sở để có thể thay đổi cách lưu trữ mà không phải sửa các hàm bên dưới.
        private Lucene.Net.Store.Directory _indexDir;
        private readonly StandardAnalyzer _analyzer;
        // Analyzer dùng khi GHI chỉ mục (IndexWriter): field "content" dùng ContentPrefixAnalyzer
        // (sinh gram tiền tố), các field khác dùng _analyzer thường. KHÔNG dùng cho việc phân
        // tích từ khóa lúc tìm (AnalyzeTerms vẫn dùng _analyzer thường bên dưới).
        private readonly Lucene.Net.Analysis.Analyzer _indexAnalyzer;
        private readonly object _writeLock = new object();

        // Tên các field trong index Lucene
        public const string FIELD_PATH = "path";                 // đường dẫn đầy đủ - khóa duy nhất, không phân tích
        // Đường dẫn đầy đủ viết THƯỜNG, không phân tích (StringField) - dùng riêng để PrefixQuery
        // lọc nhanh theo tiền tố thư mục (xem giải thích ở phần "TỐI ƯU TỐC ĐỘ" phía trên). Không
        // dùng chung FIELD_PATH cho việc này vì FIELD_PATH giữ nguyên hoa/thường (để hiển thị/mở
        // file đúng), trong khi việc so khớp tiền tố thư mục cần không phân biệt hoa/thường giống
        // hành vi StartsWith(..., OrdinalIgnoreCase) đang dùng ở nơi khác trong code.
        public const string FIELD_PATH_LOWER = "path_lower";
        public const string FIELD_FILENAME = "filename";          // tên file - có phân tích (tìm theo từng từ)
        public const string FIELD_FILENAME_KEYWORD = "filename_kw"; // tên file viết thường, nguyên văn - dùng wildcard để tìm kiểu "chứa chuỗi con" giống Contains cũ
        public const string FIELD_CONTENT = "content";            // nội dung file - có phân tích; lúc GHI dùng ContentPrefixAnalyzer (sinh sẵn gram tiền tố)
        // Lưu thời gian sửa đổi cuối cùng (LastWriteTimeUtc.Ticks, dạng chuỗi) của file tại thời điểm
        // lập chỉ mục. Dùng để so sánh ở UpdateIndex: nếu ticks không đổi -> file chưa bị sửa -> bỏ qua
        // việc đọc lại nội dung (tiết kiệm thời gian rất nhiều khi mở lại app với thư mục đã quét trước đó).
        public const string FIELD_LASTWRITE = "lastwrite_ticks";

        // Thời gian tối đa (mili-giây) cho phép trích xuất nội dung của MỘT file. Một số thư viện đọc
        // docx/pdf/ppt/xls bên thứ 3 có thể "treo" vô thời hạn với file quá lớn, bị hỏng, hoặc có cấu
        // trúc bất thường - nếu không giới hạn, cả quá trình lập chỉ mục (và cả app) sẽ đứng yên mãi ở
        // đúng file đó, y như trường hợp bị kẹt ở "Đã kiểm tra: X/Y file" không nhúc nhích. Khi vượt
        // quá thời gian này, nội dung của file đó bị bỏ qua (file vẫn được lập chỉ mục theo tên) và
        // tiến trình tiếp tục với file kế tiếp thay vì treo cứng.
        private const int ExtractionTimeoutMs = 15000; // 15 giây / file

        /// <summary>
        /// Khởi tạo dịch vụ với chỉ mục được lưu bền vững tại <paramref name="indexPath"/>.
        /// Nếu thư mục chưa có chỉ mục, Lucene sẽ tự tạo mới khi ghi lần đầu (CREATE_OR_APPEND).
        /// Nếu thư mục đã có chỉ mục từ lần chạy trước, nó sẽ được mở lại và dùng để tìm kiếm/cập nhật.
        /// Nếu chỉ mục trên đĩa được tạo bằng phiên bản cấu trúc cũ (IndexSchemaVersion khác), toàn bộ
        /// file chỉ mục cũ sẽ bị xóa để UpdateIndex lập lại từ đầu theo cách mới.
        /// </summary>
        /// <param name="indexPath">
        /// Thư mục trên đĩa để lưu chỉ mục. Nên trỏ tới %LOCALAPPDATA%\TênApp\SearchIndex
        /// (xem GetIndexStoragePath() trong Form1) để tránh vấn đề quyền ghi ở Program Files.
        /// Thư mục này phải là thư mục RIÊNG của chỉ mục vì khi đổi phiên bản cấu trúc, các file
        /// trong đó sẽ bị xóa.
        /// </param>
        public LuceneSearchService(string indexPath)
        {
            if (string.IsNullOrWhiteSpace(indexPath))
                throw new ArgumentException("indexPath không được để trống.", nameof(indexPath));

            System.IO.Directory.CreateDirectory(indexPath); // đảm bảo thư mục tồn tại trước khi FSDirectory mở

            // Nếu chỉ mục trên đĩa được tạo bằng cách lập chỉ mục cũ (analyzer/chuẩn hóa khác) thì dữ
            // liệu cũ không còn tương thích với truy vấn mới -> xóa để build lại từ đầu.
            string markerPath = Path.Combine(indexPath, IndexSchemaMarkerFile);
            bool needReset;
            try
            {
                needReset = !File.Exists(markerPath)
                            || File.ReadAllText(markerPath).Trim() != IndexSchemaVersion;
            }
            catch
            {
                needReset = true;
            }

            if (needReset)
            {
                foreach (var f in System.IO.Directory.GetFiles(indexPath))
                {
                    try { File.Delete(f); } catch { /* bỏ qua file đang bị khóa */ }
                }
                try { File.WriteAllText(markerPath, IndexSchemaVersion); } catch { }
            }

            // CharArraySet.EMPTY_SET: KHÔNG loại bỏ stop word nào (the, of, in, is...). Nếu để mặc định,
            // stop word bị bỏ khỏi truy vấn nhưng vẫn chiếm vị trí trong nội dung đã index, khiến
            // SpanNearQuery (slop = 0) không còn thấy các từ liền kề -> tìm cụm từ bị hỏng.
            _analyzer = new StandardAnalyzer(AppLuceneVersion, CharArraySet.EMPTY_SET);

            // Analyzer riêng cho field "content" lúc GHI: sinh sẵn gram tiền tố (xem
            // ContentPrefixAnalyzer/EdgeNGramPrefixFilter). Các field khác (filename, path...)
            // vẫn dùng _analyzer thường thông qua PerFieldAnalyzerWrapper.
            _indexAnalyzer = new PerFieldAnalyzerWrapper(
                _analyzer,
                new Dictionary<string, Lucene.Net.Analysis.Analyzer>
                {
                    { FIELD_CONTENT, new ContentPrefixAnalyzer(AppLuceneVersion) }
                });

            _indexDir = FSDirectory.Open(indexPath);

            // Nếu lần chạy trước ứng dụng bị đóng đột ngột (crash/kill) trong lúc đang ghi chỉ mục,
            // file khóa (write.lock) có thể còn sót lại khiến lần mở này không ghi được. Dọn nó đi.
            try
            {
                if (IndexWriter.IsLocked(_indexDir))
                    IndexWriter.Unlock(_indexDir);
            }
            catch { /* bỏ qua nếu không kiểm tra được, IndexWriter bên dưới sẽ báo lỗi rõ ràng hơn nếu cần */ }
        }

        /// <summary>
        /// Constructor không tham số: dùng thư mục tạm của hệ thống làm nơi lưu chỉ mục.
        /// Giữ lại để tương thích ngược nếu nơi nào khác trong code còn gọi "new LuceneSearchService()".
        /// Khuyến nghị dùng constructor có indexPath ở trên.
        /// </summary>
        public LuceneSearchService() : this(Path.Combine(Path.GetTempPath(), "TimKiemNhanhTaiLieu_TempIndex"))
        {
        }

        /// <summary>
        /// Chuẩn hóa văn bản về dạng Unicode NFC (dựng sẵn). Chữ tiếng Việt trích từ PDF/Word thường ở
        /// dạng tổ hợp (chữ cái + dấu rời), còn khi người dùng gõ bàn phím lại ra dạng dựng sẵn -> nếu
        /// không chuẩn hóa, hai term nhìn giống hệt nhau nhưng khác byte nên không bao giờ khớp.
        /// PHẢI áp dụng cho CẢ nội dung khi index LẪN từ khóa khi tìm.
        /// </summary>
        private static string NormalizeText(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            try { return s.Normalize(NormalizationForm.FormC); }
            catch { return s; } // chuỗi có ký tự Unicode không hợp lệ -> giữ nguyên
        }

        /// <summary>
        /// Collector tối giản cho Lucene: chỉ thu thập docId của các document khớp, KHÔNG tính điểm
        /// (scoring). Dùng để duyệt kết quả truy vấn nhanh nhất có thể, vì ta chỉ cần biết "có
        /// những document nào khớp" chứ không cần xếp hạng độ liên quan như khi tìm kiếm thông thường.
        /// </summary>
        private sealed class AllMatchesCollector : ICollector
        {
            private int _docBase;
            public List<int> DocIds { get; } = new List<int>();

            // true: cho phép Lucene trả kết quả không cần sắp theo điểm số -> nhanh hơn.
            public bool AcceptsDocsOutOfOrder => true;

            public void Collect(int doc) => DocIds.Add(doc + _docBase);

            public void SetNextReader(AtomicReaderContext context) => _docBase = context.DocBase;

            public void SetScorer(Scorer scorer) { /* không cần điểm số nên bỏ qua */ }
        }

        /// <summary>
        /// Chuẩn hóa một đường dẫn thư mục để dùng làm tiền tố so khớp an toàn: lấy đường dẫn đầy đủ,
        /// viết thường, và luôn kết thúc bằng dấu phân cách thư mục - để tránh khớp nhầm kiểu
        /// "C:\Data" khớp cả "C:\Data2".
        /// </summary>
        private static string NormalizeFolderPrefix(string folderPath)
        {
            string normalized = Path.GetFullPath(folderPath);
            if (!normalized.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                normalized += Path.DirectorySeparatorChar;
            return normalized.ToLowerInvariant();
        }

        /// <summary>
        /// Đọc từ chỉ mục trên đĩa danh sách "path -> lastwrite_ticks" nhưng CHỈ cho những document
        /// nằm trong thư mục <paramref name="normalizedFolderLower"/> (đã chuẩn hóa, viết thường).
        ///
        /// Dùng PrefixQuery trên FIELD_PATH_LOWER thay vì duyệt for (int i = 0; i < reader.MaxDoc; i++)
        /// toàn bộ index: PrefixQuery tra cứu qua term dictionary (giống B-Tree) nên chi phí chỉ tỉ lệ
        /// với số document THỰC SỰ khớp tiền tố, không phụ thuộc tổng số file đã từng lập chỉ mục của
        /// mọi thư mục/ổ đĩa khác - đây chính là phần giúp mở app / đổi thư mục nhanh hơn nhiều.
        /// </summary>
        private Dictionary<string, string> GetExistingForFolder(DirectoryReader reader, string normalizedFolderLower)
        {
            var existing = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var searcher = new IndexSearcher(reader);
            Query folderQuery = new PrefixQuery(new Term(FIELD_PATH_LOWER, normalizedFolderLower));

            var collector = new AllMatchesCollector();
            searcher.Search(folderQuery, collector);

            foreach (int docId in collector.DocIds)
            {
                var doc = reader.Document(docId);
                string path = doc.Get(FIELD_PATH);
                if (!string.IsNullOrEmpty(path))
                    existing[path] = doc.Get(FIELD_LASTWRITE);
            }

            return existing;
        }

        /// <summary>
        /// CẬP NHẬT GIA TĂNG chỉ mục dựa trên danh sách file vừa quét được (files).
        /// - File chưa từng được lập chỉ mục, hoặc đã bị sửa đổi (so sánh LastWriteTimeUtc) -> đọc lại
        ///   nội dung và cập nhật (upsert) vào chỉ mục.
        /// - File không đổi so với lần lập chỉ mục trước -> BỎ QUA việc đọc nội dung, giữ nguyên dữ
        ///   liệu cũ trong chỉ mục -> đây là phần giúp tiết kiệm thời gian ở các lần mở app sau.
        /// - File từng có trong chỉ mục nhưng không còn xuất hiện trong <paramref name="files"/>
        ///   (đã bị xóa/di chuyển khỏi thư mục đang quét) -> bị xóa khỏi chỉ mục.
        /// </summary>
        /// <param name="files">Danh sách file vừa quét được của thư mục hiện tại.</param>
        /// <param name="onFileSkipped">Callback tùy chọn, báo tên file bị bỏ qua do lỗi đọc nội dung.</param>
        /// <param name="onProgress">Callback tùy chọn, báo số file đã xử lý (kể cả file được bỏ qua vì không đổi) để cập nhật UI đếm tiến độ.</param>
        /// <param name="cancellationToken">
        /// Token để dừng giữa chừng. Khi bị hủy, những thay đổi đã Commit trước đó (nếu writer đã
        /// commit theo lô) vẫn được giữ; những gì chưa commit ở lần Commit cuối trong hàm này sẽ được
        /// commit ngay trước khi thoát để không mất dữ liệu đã xử lý.
        /// </param>
        /// <returns>Số lượng file đã được xử lý (dù thay đổi hay không, hay bị lỗi) trước khi dừng.</returns>
        /// <param name="folderPath">
        /// Đường dẫn gốc của thư mục đang được quét (chính là tham số truyền vào ScanDirectoryRecursive
        /// ở Form1). BẮT BUỘC phải truyền đúng để hàm biết phạm vi "thư mục hiện tại" là gì.
        ///
        /// TẠI SAO CẦN THAM SỐ NÀY: chỉ mục trên đĩa có thể chứa dữ liệu của NHIỀU thư mục khác nhau
        /// mà người dùng từng chọn qua các lần dùng app trước đó (đó chính là lý do nó hữu ích - đổi
        /// qua lại giữa các thư mục không phải index lại từ đầu). Nếu không giới hạn phạm vi, bước
        /// "xóa file không còn tồn tại" bên dưới sẽ hiểu nhầm toàn bộ file của MỌI thư mục khác (không
        /// nằm trong lần quét hiện tại) là "đã bị xóa" và xóa sạch chúng khỏi chỉ mục - tức là mỗi lần
        /// đổi thư mục sẽ vô tình xóa mất chỉ mục của thư mục cũ. folderPath dùng để chỉ xóa các mục
        /// thực sự nằm BÊN TRONG thư mục đang quét, giữ nguyên hoàn toàn dữ liệu của các thư mục khác.
        ///
        /// Đồng thời, tham số này cũng chính là tiền tố dùng cho PrefixQuery ở bước đọc "existing" bên
        /// dưới, nên việc tra cứu trạng thái cũ cũng tự động chỉ giới hạn trong đúng thư mục này.
        /// </param>
        public int UpdateIndex(
            string folderPath,
            IEnumerable<FileInfo> files,
            Action<string> onFileSkipped = null,
            IProgress<int> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            int processed = 0;

            // Chuẩn hóa đường dẫn thư mục để so khớp tiền tố (prefix) an toàn: dùng đường dẫn đầy đủ,
            // viết thường, và luôn có dấu phân cách ở cuối, để "C:\Data" không vô tình khớp nhầm với
            // "C:\Data2". Cùng một chuẩn hóa được dùng cho cả PrefixQuery (đọc existing) lẫn bước xóa
            // file không còn tồn tại bên dưới, đảm bảo hai bước luôn nhất quán về phạm vi thư mục.
            string normalizedFolder = Path.GetFullPath(folderPath);
            if (!normalizedFolder.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                normalizedFolder += Path.DirectorySeparatorChar;
            string normalizedFolderLower = normalizedFolder.ToLowerInvariant();

            // Dùng _indexAnalyzer (PerFieldAnalyzerWrapper) thay vì _analyzer trực tiếp, để field
            // content được lập chỉ mục bằng ContentPrefixAnalyzer (sinh gram tiền tố), còn các field
            // khác vẫn dùng _analyzer thường.
            var config = new IndexWriterConfig(AppLuceneVersion, _indexAnalyzer)
            {
                // CREATE_OR_APPEND: nếu chưa có chỉ mục trên đĩa thì tạo mới, nếu đã có thì mở tiếp
                // để cập nhật (khác với CREATE của BuildIndex cũ, vốn luôn xóa sạch làm lại).
                OpenMode = OpenMode.CREATE_OR_APPEND
            };

            lock (_writeLock)
            {
                using (var writer = new IndexWriter(_indexDir, config))
                {
                    // Đọc trạng thái đã lập chỉ mục từ lần trước: path -> lastwrite_ticks đã lưu.
                    //
                    // QUAN TRỌNG (tối ưu tốc độ): CHỈ đọc các document thuộc ĐÚNG thư mục đang quét,
                    // bằng PrefixQuery trên FIELD_PATH_LOWER (xem GetExistingForFolder), thay vì
                    // duyệt for (int i = 0; i < reader.MaxDoc; i++) toàn bộ chỉ mục như trước đây.
                    // Nhờ vậy chi phí bước này không còn phụ thuộc vào tổng số file đã từng lập chỉ
                    // mục ở TẤT CẢ các ổ đĩa/thư mục khác - đây chính là nguyên nhân gây lag/mất
                    // nhiều thời gian khi mở app hoặc đổi sang thư mục/ổ đĩa khác mà bạn gặp phải.
                    var existing = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (DirectoryReader.IndexExists(_indexDir))
                    {
                        using (var reader = DirectoryReader.Open(_indexDir))
                        {
                            existing = GetExistingForFolder(reader, normalizedFolderLower);
                        }
                    }

                    var currentPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var fi in files)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        currentPaths.Add(fi.FullName);
                        string ticksNow = fi.LastWriteTimeUtc.Ticks.ToString();

                        bool needUpdate = !existing.TryGetValue(fi.FullName, out var oldTicks)
                                           || !string.Equals(oldTicks, ticksNow, StringComparison.Ordinal);

                        if (needUpdate)
                        {
                            try
                            {
                                // Dùng ExtractContentSafe (có timeout) thay vì gọi ExtractContent trực
                                // tiếp, để một file "treo" (lỗi thư viện đọc docx/pdf/ppt/xls/rtf) không
                                // làm đứng cả quá trình lập chỉ mục. Kết quả đã được chuẩn hóa NFC.
                                string content = ExtractContentSafe(fi, onFileSkipped);
                                string nameNfc = NormalizeText(fi.Name);
                                var doc = new Document
                                {
                                    new StringField(FIELD_PATH, fi.FullName, Lucene.Net.Documents.Field.Store.YES),
                                    new StringField(FIELD_PATH_LOWER, fi.FullName.ToLowerInvariant(), Lucene.Net.Documents.Field.Store.NO),
                                    new TextField(FIELD_FILENAME, nameNfc, Lucene.Net.Documents.Field.Store.NO),
                                    new StringField(FIELD_FILENAME_KEYWORD, nameNfc.ToLowerInvariant(), Lucene.Net.Documents.Field.Store.NO),
                                    new TextField(FIELD_CONTENT, content ?? string.Empty, Lucene.Net.Documents.Field.Store.NO),
                                    new StringField(FIELD_LASTWRITE, ticksNow, Lucene.Net.Documents.Field.Store.YES)
                                };
                                // UpdateDocument = xóa document cũ có cùng FIELD_PATH (nếu có) rồi thêm
                                // document mới -> hoạt động như "upsert", vừa dùng được cho file mới
                                // lẫn file đã có trong chỉ mục từ trước.
                                writer.UpdateDocument(new Term(FIELD_PATH, fi.FullName), doc);
                            }
                            catch
                            {
                                onFileSkipped?.Invoke(fi.Name);
                            }
                        }
                        // else: file không đổi -> không làm gì cả, giữ nguyên document cũ trong chỉ mục.

                        processed++;
                        if (processed % 10 == 0)
                            onProgress?.Report(processed);
                    }

                    // Xóa khỏi chỉ mục những file đã từng được lập chỉ mục nhưng không còn xuất hiện
                    // trong lần quét hiện tại (đã bị xóa/di chuyển khỏi thư mục).
                    //
                    // Nhờ existing đã được GetExistingForFolder() giới hạn sẵn trong đúng thư mục đang
                    // quét (qua PrefixQuery ở trên), mọi key trong existing chắc chắn thuộc
                    // normalizedFolder rồi, nên không cần kiểm tra StartsWith lại ở đây nữa - nhưng
                    // vẫn giữ lại điều kiện này như một lớp bảo vệ an toàn (defensive check), phòng
                    // trường hợp dữ liệu index có gì đó bất thường, với chi phí không đáng kể.
                    foreach (var oldPath in existing.Keys)
                    {
                        bool belongsToCurrentFolder = oldPath.StartsWith(normalizedFolder, StringComparison.OrdinalIgnoreCase);
                        if (belongsToCurrentFolder && !currentPaths.Contains(oldPath))
                            writer.DeleteDocuments(new Term(FIELD_PATH, oldPath));
                    }

                    writer.Commit();
                    onProgress?.Report(processed);
                }
            }

            return processed;
        }

        /// <summary>
        /// Lập chỉ mục lại TOÀN BỘ danh sách file (bỏ toàn bộ chỉ mục cũ, làm lại từ đầu).
        /// Giữ lại để dùng khi cần "làm mới hoàn toàn" chỉ mục; việc mở app hằng ngày nên dùng
        /// UpdateIndex() ở trên để tận dụng cơ chế cập nhật gia tăng, nhanh hơn nhiều.
        /// </summary>
        public int BuildIndex(
            IEnumerable<FileInfo> files,
            Action<string> onFileSkipped = null,
            IProgress<int> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            int processed = 0;

            // Dùng _indexAnalyzer (PerFieldAnalyzerWrapper) như UpdateIndex, để field content cũng
            // được lập chỉ mục bằng ContentPrefixAnalyzer khi rebuild toàn bộ.
            var config = new IndexWriterConfig(AppLuceneVersion, _indexAnalyzer)
            {
                OpenMode = OpenMode.CREATE // luôn xóa sạch và tạo mới
            };

            lock (_writeLock)
            {
                using (var writer = new IndexWriter(_indexDir, config))
                {
                    foreach (var fi in files)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        try
                        {
                            string content = ExtractContentSafe(fi, onFileSkipped);
                            string nameNfc = NormalizeText(fi.Name);
                            var doc = new Document
                            {
                                new StringField(FIELD_PATH, fi.FullName, Lucene.Net.Documents.Field.Store.YES),
                                new StringField(FIELD_PATH_LOWER, fi.FullName.ToLowerInvariant(), Lucene.Net.Documents.Field.Store.NO),
                                new TextField(FIELD_FILENAME, nameNfc, Lucene.Net.Documents.Field.Store.NO),
                                new StringField(FIELD_FILENAME_KEYWORD, nameNfc.ToLowerInvariant(), Lucene.Net.Documents.Field.Store.NO),
                                new TextField(FIELD_CONTENT, content ?? string.Empty, Lucene.Net.Documents.Field.Store.NO),
                                new StringField(FIELD_LASTWRITE, fi.LastWriteTimeUtc.Ticks.ToString(), Lucene.Net.Documents.Field.Store.YES)
                            };
                            writer.AddDocument(doc);
                        }
                        catch
                        {
                            onFileSkipped?.Invoke(fi.Name);
                        }

                        processed++;
                        if (processed % 10 == 0)
                            onProgress?.Report(processed);
                    }

                    writer.Commit();
                    onProgress?.Report(processed);
                }
            }

            return processed;
        }

        /// <summary>
        /// Xóa một tài liệu khỏi index (gọi khi người dùng xóa file trong btndelete_Click)
        /// để lần tìm kiếm tiếp theo không trả về file đã không còn tồn tại.
        /// </summary>
        public void RemoveDocument(string fullPath)
        {
            lock (_writeLock)
            {
                if (_indexDir == null) return;

                var config = new IndexWriterConfig(AppLuceneVersion, _indexAnalyzer)
                {
                    OpenMode = OpenMode.CREATE_OR_APPEND
                };

                using (var writer = new IndexWriter(_indexDir, config))
                {
                    writer.DeleteDocuments(new Term(FIELD_PATH, fullPath));
                    writer.Commit();
                }
            }
        }

        /// <summary>
        /// Gọi ExtractContent(fi) trên một Task riêng và CHỜ CÓ GIỚI HẠN THỜI GIAN
        /// (ExtractionTimeoutMs). Đây là lớp bảo vệ chung cho MỌI định dạng file, phòng trường hợp
        /// một thư viện đọc file (PdfPig, ExcelDataReader, SautinSoft, HtmlAgilityPack...) bị "treo"
        /// với một file cụ thể (quá lớn, cấu trúc bất thường, hỏng dữ liệu...).
        ///
        /// LƯU Ý: đây KHÔNG phải hủy (cancel) thật sự - .NET không có cách an toàn để "dừng cứng" một
        /// luồng đang chạy code không hỗ trợ hủy. Task chạy ExtractContent(fi) cho file bị treo vẫn sẽ
        /// tiếp tục chạy ngầm cho tới khi tự kết thúc (hoặc tới khi app bị đóng hẳn); nhưng vòng lặp
        /// lập chỉ mục KHÔNG chờ nó nữa mà bỏ qua nội dung file này và đi tiếp file kế tiếp ngay - đó
        /// là điều quan trọng nhất để tránh cả quá trình bị đứng như trong ảnh bạn gửi.
        ///
        /// Nội dung trả về luôn được chuẩn hóa Unicode NFC (xem NormalizeText).
        /// </summary>
        private string ExtractContentSafe(FileInfo fi, Action<string> onFileSkipped)
        {
            try
            {
                var task = Task.Run(() => ExtractContent(fi));
                if (task.Wait(ExtractionTimeoutMs))
                {
                    // Hoàn thành trong thời gian cho phép -> lấy kết quả bình thường.
                    // Nếu bản thân ExtractContent ném lỗi, task.Result sẽ ném lại lỗi đó ở đây,
                    // được bắt bởi catch bên dưới -> coi như file bị bỏ qua, không ảnh hưởng file khác.
                    return NormalizeText(task.Result);
                }

                // Quá thời gian cho phép -> coi như "treo", bỏ qua nội dung của file này (file vẫn
                // được lập chỉ mục theo tên, chỉ không tìm được theo nội dung) và báo cho người dùng
                // biết qua onFileSkipped để có thể xem lại danh sách file bị bỏ qua nếu cần.
                onFileSkipped?.Invoke(fi.Name + " (quá thời gian trích xuất nội dung, đã bỏ qua)");
                return string.Empty;
            }
            catch
            {
                onFileSkipped?.Invoke(fi.Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// Trích nội dung văn bản của file theo phần mở rộng.
        /// Định dạng không hỗ trợ trả về chuỗi rỗng (vẫn tìm được theo tên file bình thường).
        /// </summary>
        private string ExtractContent(FileInfo fi)
        {
            if (fi.Length > MaxContentFileSizeBytes)
                return string.Empty; // file quá lớn -> bỏ qua trích nội dung để tránh treo UI

            string ext = fi.Extension.ToLowerInvariant();

            switch (ext)
            {
                case ".docx":
                    return ExtractDocxContent(fi.FullName);
                case ".doc":
                    return ExtractDocContent(fi.FullName);
                case ".xlsx":
                case ".xls":
                    return ExtractExcelContent(fi.FullName);
                case ".pptx":
                case ".ppt":
                    return ExtractPptContent(fi.FullName);
                case ".rtf":
                    return ExtractRtfContent(fi.FullName);
                case ".html":
                case ".htm":
                    return ExtractHtmlContent(fi.FullName);
                case ".json":
                    return ExtractJsonContent(fi.FullName);
                case ".pdf":
                    return ExtractPdfContent(fi.FullName);
                case ".txt":
                case ".md":
                case ".csv":
                case ".log":
                    return SafeReadText(fi.FullName);
                default:
                    return string.Empty;
            }
        }

        private string ExtractDocxContent(string path)
        {
            var sb = new StringBuilder();
            using (var docPackage = WordprocessingDocument.Open(path, false))
            {
                var body = docPackage.MainDocumentPart?.Document?.Body;
                if (body == null) return string.Empty;

                foreach (var text in body.Descendants<WordXml.Text>())
                {
                    sb.Append(text.Text);
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }

        private string ExtractPdfContent(string path)
        {
            var sb = new StringBuilder();
            using (var pdf = PdfDocument.Open(path))
            {
                foreach (var page in pdf.GetPages())
                {
                    // GetWords() nhóm ký tự thành từ dựa trên khoảng cách hình học thực tế,
                    // thay vì nối thô theo thứ tự lưu trong file (page.Text) — cách cũ khiến
                    // các slide PDF không có dấu cách rõ ràng bị dính liền thành 1 token dài,
                    // làm hỏng hoàn toàn tìm kiếm theo cụm từ.
                    foreach (var word in page.GetWords())
                    {
                        sb.Append(word.Text);
                        sb.Append(' ');
                    }
                    sb.Append('\n');
                }
            }
            return sb.ToString();
        }
        private string SafeReadText(string path)
        {
            try { return File.ReadAllText(path); }
            catch { return string.Empty; }
        }
        //bổ sung đọc file word cũ đuôi .doc
        private string ExtractDocContent(string path)
        {
            try
            {
                // Đọc file Word cũ .doc
                DocumentCore document = DocumentCore.Load(path);

                // Lấy toàn bộ nội dung text
                return document.Content.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đọc file DOC: {ex.Message}");
                return string.Empty;
            }
        }
        //Excel .xlsx / .xls
        private string ExtractExcelContent(string path)
        {
            try
            {
                var sb = new StringBuilder();

                using (var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        // Đọc từng dòng
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                object value = reader.GetValue(i);

                                if (value != null)
                                {
                                    string text = value.ToString();

                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        sb.Append(text);
                                        sb.Append(' ');
                                    }
                                }
                            }

                            sb.AppendLine();
                        }
                    }
                    while (reader.NextResult());
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đọc Excel: {path}");
                Console.WriteLine($"Chi tiết: {ex.Message}");

                return string.Empty;
            }
        }
        //PowerPoint .pptx / .ppt
        private string ExtractPptContent(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return string.Empty;

                // SautinSoft.Document tự nhận diện định dạng .ppt / .pptx
                DocumentCore document = DocumentCore.Load(path);

                // Lấy toàn bộ nội dung văn bản
                return document.Content.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đọc PowerPoint: {path}");
                Console.WriteLine($"Chi tiết: {ex.Message}");

                return string.Empty;
            }
        }
        //RTF .rtf
        //
        // QUAN TRỌNG: RichTextBox là control Win32 (RichEdit) - việc gọi LoadFile() yêu cầu chạy trên
        // một luồng STA (Single-Threaded Apartment) có khả năng tạo window handle. Nhưng hàm này được
        // gọi từ bên trong Task.Run (luồng ThreadPool, mặc định là MTA) trong quá trình lập chỉ mục ->
        // nếu gọi trực tiếp như bản cũ, việc tạo handle của RichTextBox có thể bị TREO VÔ THỜI HẠN,
        // đúng như hiện tượng "đứng ở X/Y file" mà bạn gặp. Cách khắc phục đúng là tự tạo một Thread
        // MỚI, đặt ApartmentState = STA cho thread đó, chạy toàn bộ thao tác RichTextBox bên trong nó,
        // rồi chờ (Join) có giới hạn thời gian.
        private string ExtractRtfContent(string path)
        {
            string result = string.Empty;

            var staThread = new Thread(() =>
            {
                try
                {
                    using (var rtb = new System.Windows.Forms.RichTextBox())
                    {
                        rtb.LoadFile(path, System.Windows.Forms.RichTextBoxStreamType.RichText);
                        result = rtb.Text;
                    }
                }
                catch
                {
                    result = string.Empty;
                }
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.IsBackground = true; // không cản trở app đóng nếu thread này kẹt lại
            staThread.Start();

            // Chờ tối đa vài giây riêng cho bước đọc RTF (độc lập với ExtractionTimeoutMs bên ngoài,
            // để nếu thread STA vẫn đang chạy dở thì ExtractContentSafe ở lớp ngoài sẽ là lưới an toàn
            // cuối cùng). Nếu không xong trong thời gian này, coi như không lấy được nội dung.
            staThread.Join(TimeSpan.FromSeconds(10));
            return result;
        }
        //HTML .html / .htm
        private string ExtractHtmlContent(string path)
        {
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(path);
                return doc.DocumentNode.InnerText;
            }
            catch
            {
                return string.Empty;
            }
        }
        //JSON .json
        private string ExtractJsonContent(string path)
        {
            try
            {
                string raw = File.ReadAllText(path);
                var doc = System.Text.Json.JsonDocument.Parse(raw);
                return raw; // hoặc duyệt để lấy text cụ thể
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Tìm kiếm theo từ khóa và chế độ đã chọn.
        /// Trả về tập hợp đường dẫn đầy đủ (FullName) của các file khớp.
        /// </summary>
        /// <param name="keyword">Từ khóa người dùng nhập.</param>
        /// <param name="mode">Tên file / Nội dung / Cả hai.</param>
        /// <param name="exactWords">
        /// Chỉ áp dụng cho tìm trong NỘI DUNG.
        /// false (mặc định): các từ đầu khớp chính xác liền kề đúng thứ tự, riêng TỪ CUỐI khớp theo
        ///   tiền tố -> "hợp ngữ l" tìm ra "hợp ngữ là một...". Chế độ này đã bao gồm cả trường hợp
        ///   gõ đủ từ ("hợp ngữ là" vẫn ra "hợp ngữ là một").
        /// true: từ cuối cũng phải khớp NGUYÊN TỪ -> "hợp ngữ là" KHÔNG khớp "hợp ngữ làm".
        /// </param>
        public HashSet<string> Search(string keyword, SearchMode mode, bool exactWords = false)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(keyword)) return result;

            lock (_writeLock)
            {
                if (_indexDir == null || !DirectoryReader.IndexExists(_indexDir))
                    return result;

                using (var reader = DirectoryReader.Open(_indexDir))
                {
                    var searcher = new IndexSearcher(reader);

                    // Chuẩn hóa NFC để khớp với dữ liệu đã được chuẩn hóa lúc lập chỉ mục.
                    string trimmed = NormalizeText(keyword.Trim());

                    Query query;
                    switch (mode)
                    {
                        case SearchMode.FileName:
                            query = BuildFileNameQuery(trimmed);
                            break;

                        case SearchMode.Content:
                            query = BuildContentQuery(trimmed, exactWords);
                            break;

                        default: // Both
                            var boolQuery = new BooleanQuery
                            {
                                { BuildFileNameQuery(trimmed), Occur.SHOULD },
                                { BuildContentQuery(trimmed, exactWords), Occur.SHOULD }
                            };
                            query = boolQuery;
                            break;
                    }

                    // Lấy TOÀN BỘ tài liệu khớp (không còn giới hạn 2000 như Search(query, 2000)).
                    // Ta chỉ cần biết file nào khớp, không cần xếp hạng điểm số.
                    var collector = new AllMatchesCollector();
                    searcher.Search(query, collector);

                    foreach (int docId in collector.DocIds)
                    {
                        string path = reader.Document(docId).Get(FIELD_PATH);
                        if (!string.IsNullOrEmpty(path)) result.Add(path);
                    }
                }
            }

            return result;
        }

        // Wildcard trên field không phân tích (FIELD_FILENAME_KEYWORD) để tìm kiểu "chứa chuỗi con"
        // giống hành vi Contains() của bản cũ, không phụ thuộc việc tách từ.
        private Query BuildFileNameQuery(string keyword)
        {
            string term = keyword.ToLowerInvariant();
            return new WildcardQuery(new Term(FIELD_FILENAME_KEYWORD, "*" + term + "*"));
        }

        /// <summary>
        /// Tìm trong nội dung theo kiểu "cụm từ + tiền tố ở từ cuối":
        ///   "ngôn ngữ là m"  ->  khớp "ngôn ngữ là một", "ngôn ngữ là mọi", ...
        /// Các từ trước phải xuất hiện LIỀN KỀ đúng thứ tự; riêng từ cuối:
        ///   - exactWords = false: chỉ cần bắt đầu bằng những ký tự người dùng đã gõ (gõ tới đâu tìm tới đó).
        ///   - exactWords = true : phải khớp nguyên từ.
        ///
        /// CÁCH KHỚP TIỀN TỐ: field FIELD_CONTENT được lập chỉ mục bằng ContentPrefixAnalyzer (xem
        /// EdgeNGramPrefixFilter.cs), tức mỗi từ đã có sẵn TẤT CẢ tiền tố của nó (đánh dấu bằng
        /// EdgeNGramPrefixFilter.MarkerChar) xếp chồng cùng vị trí với từ đầy đủ. Nhờ vậy tra tiền tố
        /// chỉ là TermQuery thẳng vào term dictionary - không còn PrefixQuery/wildcard mở rộng lúc tìm.
        ///
        /// DÙNG MultiPhraseQuery THAY VÌ SpanNearQuery: SpanNearQuery/Span* là framework tổng quát
        /// (hỗ trợ lồng nhau, khoảng cách phức tạp...) nên có overhead lớn hơn hẳn so với
        /// PhraseQuery/MultiPhraseQuery - vốn được Lucene cài đặt tối ưu RIÊNG cho đúng bài toán "các
        /// từ liền kề theo vị trí" như ở đây, nên nói chung chạy nhẹ hơn. MultiPhraseQuery cho phép
        /// thêm NHIỀU term vào CÙNG một vị trí trong cụm từ (giống kiểu "OR tại vị trí đó") - dùng
        /// đúng vào việc chứa cả "từ đầy đủ" (không marker) lẫn "gram tiền tố + marker" ở vị trí từ
        /// cuối, thay cho SpanOrQuery trước đây. Các từ ở đầu câu vẫn chỉ thêm 1 term/vị trí (khớp
        /// chính xác, không marker) nên không bị ăn nhầm gram của từ dài hơn.
        /// </summary>
        private Query BuildContentQuery(string keyword, bool exactWords)
        {
            var terms = AnalyzeTerms(FIELD_CONTENT, keyword);
            if (terms.Count == 0) return new BooleanQuery();

            var phraseQuery = new MultiPhraseQuery();

            // Các từ đầu câu: mỗi vị trí chỉ 1 term - khớp CHÍNH XÁC (không marker nên không bị
            // ăn nhầm gram tiền tố của một từ dài hơn tình cờ trùng ký tự đầu).
            for (int i = 0; i < terms.Count - 1; i++)
                phraseQuery.Add(new Term(FIELD_CONTENT, terms[i]));

            string last = terms[terms.Count - 1];
            if (exactWords)
            {
                // Từ cuối phải khớp NGUYÊN TỪ.
                phraseQuery.Add(new Term(FIELD_CONTENT, last));
            }
            else
            {
                // Từ cuối: chấp nhận CẢ HAI khả năng ở cùng 1 vị trí - "gõ đủ nguyên từ" (term
                // không marker) HOẶC "đang gõ dở, mới là tiền tố" (term có marker). Đây chính là
                // điểm MultiPhraseQuery thay thế SpanOrQuery trước đây.
                phraseQuery.Add(new[]
                {
                    new Term(FIELD_CONTENT, last),
                    new Term(FIELD_CONTENT, last + EdgeNGramPrefixFilter.MarkerChar)
                });
            }

            // Slop = 0: các từ phải sát nhau, đúng thứ tự người dùng gõ (giống SpanNearQuery
            // slop=0, inOrder=true trước đây). Nếu muốn nới lỏng (cho phép xen 1 từ), đổi thành 1.
            phraseQuery.Slop = 0;

            return phraseQuery;
        }

        // Tách từ khóa thành các token đúng như cách StandardAnalyzer đã tách khi lập chỉ mục
        // (cùng phép lowercase/tách từ) để truy vấn so khớp chính xác với dữ liệu đã index.
        // LƯU Ý: dùng _analyzer (StandardAnalyzer thường), KHÔNG dùng _indexAnalyzer/
        // ContentPrefixAnalyzer ở đây - ta chỉ cần tách từ khóa người dùng gõ thành các từ, KHÔNG
        // muốn tự sinh gram cho chính từ khóa (việc thêm marker cho từ cuối được làm thủ công ở
        // BuildContentQuery, chỉ đúng 1 lần, không lặp lại cho mọi độ dài như ContentPrefixAnalyzer).
        private List<string> AnalyzeTerms(string field, string text)
        {
            var terms = new List<string>();
            using (var tokenStream = _analyzer.GetTokenStream(field, text))
            {
                var termAttr = tokenStream.AddAttribute<Lucene.Net.Analysis.TokenAttributes.ICharTermAttribute>();
                tokenStream.Reset();
                while (tokenStream.IncrementToken())
                {
                    terms.Add(termAttr.ToString());
                }
                tokenStream.End();
            }
            return terms;
        }

        public void Dispose()
        {
            lock (_writeLock)
            {
                _indexDir?.Dispose();
            }
            _analyzer?.Dispose();
        }
    }
}