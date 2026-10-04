using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Util;
using MathNet.Numerics.Distributions;
using Net.Pkcs11Interop.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace TimKiemNhanhTaiLieu
{
    /// <summary>
    /// EdgeNGramPrefixFilter — bộ lọc token tùy chỉnh
    ///Đây là một TokenFilter chạy trong lúc lập chỉ mục(không chạy lúc tìm kiếm). Với mỗi từ trong nội dung file, ví dụ từ "ngôn", nó không chỉ lập chỉ mục từ đầy đủ mà còn sinh thêm mọi tiền tố của từ đó, đánh dấu bằng ký tự đặc biệt \u0001 (marker) để phân biệt với từ thật:
      ///"ngôn" -> lập chỉ mục thành:
      ///"n\u0001"
      ///"ng\u0001"
      ///"ngô\u0001"
      ///"ngôn"      (từ đầy đủ, không marker)
    /// TokenFilter tùy chỉnh dùng khi LẬP CHỈ MỤC field nội dung (content), thay thế cho việc
    /// mở rộng tiền tố bằng PrefixQuery lúc TÌM (vốn gây lag vì phải mở rộng toàn bộ term khớp
    /// mỗi lần gõ phím).
    ///
    /// Với mỗi token đầu vào (đã qua StandardTokenizer + StandardFilter + LowerCaseFilter),
    /// filter này phát ra:
    ///   1. Từ ĐẦY ĐỦ, giữ nguyên không đổi (posIncrement bình thường) - dùng để so khớp
    ///      CHÍNH XÁC (dùng cho các từ ở ĐẦU câu trong SpanNearQuery, và cho trường hợp
    ///      exactWords = true ở từ cuối).
    ///   2. Các TIỀN TỐ của từ đó (từ 1 tới len-1 ký tự), mỗi tiền tố được thêm một ký tự
    ///      đánh dấu (MarkerChar) ở cuối, và XẾP CHỒNG cùng vị trí với từ gốc (posIncrement = 0)
    ///      - dùng để so khớp THEO TIỀN TỐ (dùng cho từ CUỐI khi người dùng còn đang gõ dở).
    ///
    /// Ý NGHĨA CỦA KÝ TỰ ĐÁNH DẤU:
    /// Lucene bắt buộc mọi vế trong một SpanNearQuery phải thuộc CÙNG một field, nên không thể
    /// tách riêng "field khớp chính xác" và "field khớp tiền tố" rồi gộp lại trong một
    /// SpanNearQuery. Giải pháp là nhét cả hai loại vào CÙNG một field, cùng một vị trí, và
    /// dùng ký tự đánh dấu để phân biệt: SpanTermQuery KHÔNG có marker chỉ khớp từ đầy đủ (không
    /// bị "ăn nhầm" tiền tố của một từ dài hơn tình cờ trùng); SpanTermQuery CÓ marker chỉ khớp
    /// đúng phần tiền tố. Nhờ đó, các từ ở đầu câu trong SpanNearQuery vẫn giữ đúng ngữ nghĩa
    /// "phải khớp chính xác", không bị lây lan hành vi khớp tiền tố sang chúng.
    ///
    /// ĐÁNH ĐỔI: index cho field content sẽ to hơn (một từ dài N ký tự sinh ra ~N term thay vì
    /// 1), nhưng đây là chi phí TRẢ MỘT LẦN lúc lập chỉ mục (và chỉ cho file mới/đã sửa, nhờ
    /// UpdateIndex gia tăng), thay vì trả lại MỖI LẦN TÌM KIẾM như PrefixQuery trước đây.
    /// </summary>
    public sealed class EdgeNGramPrefixFilter : TokenFilter
    {
        /// <summary>
        /// Ký tự dùng để đánh dấu một token là "tiền tố" (gram), phân biệt với từ đầy đủ.
        /// Dùng ký tự điều khiển (U+0001) vì gần như không bao giờ xuất hiện trong văn bản thật,
        /// nên không lo trùng với nội dung tài liệu.
        /// </summary>
        public const char MarkerChar = '\u0001';

        private readonly int _minGram;
        private readonly int _maxGram;

        private readonly ICharTermAttribute _termAtt;
        private readonly IPositionIncrementAttribute _posIncAtt;
        private readonly IOffsetAttribute _offsetAtt;

        // Danh sách các gram tiền tố còn phải phát ra cho token HIỆN TẠI (không tính từ đầy đủ,
        // vì từ đầy đủ đã được phát ngay ở lần IncrementToken() đọc token đó).
        private readonly List<string> _pendingGrams = new List<string>();
        private int _pendingIndex;
        private int _savedStartOffset;
        private int _savedEndOffset;

        /// <param name="input">TokenStream phía trước (đã lowercase, đã chuẩn hóa).</param>
        /// <param name="minGram">
        /// Độ dài tiền tố ngắn nhất được đánh chỉ mục. Để 1 nếu muốn hỗ trợ gõ 1 ký tự
        /// (ví dụ "l" khớp "là"), đổi lên 2 nếu index quá to và chấp nhận bỏ hỗ trợ 1 ký tự.
        /// </param>
        /// <param name="maxGram">
        /// Độ dài tiền tố dài nhất được đánh chỉ mục cho một từ (để tránh sinh quá nhiều gram
        /// với từ rất dài, ví dụ URL hay chuỗi không có khoảng trắng). Từ đầy đủ vẫn luôn được
        /// lập chỉ mục dù dài hơn maxGram; chỉ các gram TRUNG GIAN bị giới hạn.
        /// </param>
        public EdgeNGramPrefixFilter(TokenStream input, int minGram = 1, int maxGram = 24)
            : base(input)
        {
            _minGram = minGram;
            _maxGram = maxGram;
            _termAtt = AddAttribute<ICharTermAttribute>();
            _posIncAtt = AddAttribute<IPositionIncrementAttribute>();
            _offsetAtt = AddAttribute<IOffsetAttribute>();
        }

        public override bool IncrementToken()
        {
            // Nếu token trước đó còn gram tiền tố chưa phát hết -> phát tiếp gram kế tiếp,
            // KHÔNG đọc token mới từ input (giữ posIncrement = 0 để xếp chồng cùng vị trí).
            if (_pendingIndex < _pendingGrams.Count)
            {
                string gram = _pendingGrams[_pendingIndex++];
                ClearAttributes();
                _termAtt.SetEmpty().Append(gram).Append(MarkerChar);
                _posIncAtt.PositionIncrement = 0;
                _offsetAtt.SetOffset(_savedStartOffset, _savedEndOffset);
                return true;
            }

            // Đọc token thật tiếp theo từ chuỗi filter phía trước.
            if (!m_input.IncrementToken())
                return false;

            string term = _termAtt.ToString();
            _savedStartOffset = _offsetAtt.StartOffset;
            _savedEndOffset = _offsetAtt.EndOffset;

            // Chuẩn bị sẵn danh sách gram tiền tố (KHÔNG bao gồm chính từ đầy đủ) để phát ra ở
            // các lần gọi IncrementToken() kế tiếp. term.Length - 1 vì không lặp tới độ dài bằng
            // chính từ đó (từ đầy đủ đã được phát ở lần gọi này rồi, không cần lặp lại có marker).
            _pendingGrams.Clear();
            _pendingIndex = 0;
            int upper = Math.Min(term.Length - 1, _maxGram);
            for (int len = _minGram; len <= upper; len++)
                _pendingGrams.Add(term.Substring(0, len));

            // Từ ĐẦY ĐỦ: attribute hiện đã chứa đúng term/offset/posIncrement gốc từ m_input,
            // không cần chỉnh gì thêm - trả về luôn.
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            _pendingGrams.Clear();
            _pendingIndex = 0;
        }

        public override void End()
        {
            base.End();
        }
    }

    /// <summary>
    /// Analyzer riêng CHỈ dùng khi LẬP CHỈ MỤC field content (không dùng lúc phân tích từ khóa
    /// tìm kiếm - lúc tìm vẫn dùng StandardAnalyzer thường qua AnalyzeTerms trong
    /// LuceneSearchService, để không tự sinh gram cho chính từ khóa người dùng gõ).
    ///
    /// Chuỗi filter: StandardTokenizer -> StandardFilter -> LowerCaseFilter -> EdgeNGramPrefixFilter.
    /// KHÔNG dùng StopFilter (giữ lại toàn bộ từ, giống _analyzer chính trong LuceneSearchService),
    /// để các từ luôn liền kề đúng vị trí khi so khớp SpanNearQuery (slop = 0).
    /// </summary>
    public sealed class ContentPrefixAnalyzer : Analyzer
    {
        private readonly LuceneVersion _version;
        private readonly int _minGram;
        private readonly int _maxGram;

        public ContentPrefixAnalyzer(LuceneVersion version, int minGram = 1, int maxGram = 24)
        {
            _version = version;
            _minGram = minGram;
            _maxGram = maxGram;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(_version, reader);
            TokenStream stream = new StandardFilter(_version, tokenizer);
            stream = new LowerCaseFilter(_version, stream);
            stream = new EdgeNGramPrefixFilter(stream, _minGram, _maxGram);
            return new TokenStreamComponents(tokenizer, stream);
        }
    }
}