Giới thiệu
TimKiemNhanhTaiLieu là ứng dụng desktop chạy trên Windows (.NET Framework 4.7.2) phục vụ tìm kiếm nhanh tài liệu trên máy local. Ứng dụng dùng cơ chế lập chỉ mục và tra cứu (index + search) để trả về kết quả nhanh với hỗ trợ nhiều định dạng tài liệu (PDF, DOC/DOCX, XLS/XLSX, HTML, v.v.).
Giao diện
•	Ứng dụng là ứng dụng giao diện Windows Forms (Form1.cs, Settingsform.cs).
•	Sử dụng SunnyUI để làm giao diện/tương tác (controls hiện đại).
Tính năng chính
•	Lập chỉ mục (index) thư mục/tập tin tài liệu trên ổ cứng.
•	Tìm kiếm theo tên file, nội dung file.
•	Hỗ trợ nhiều định dạng tài liệu: PDF, Word (DOC/DOCX), Excel, HTML, văn bản thuần, v.v.
•	Xử lý và trích xuất text từ các định dạng phức tạp (sử dụng thư viện đọc tài liệu).
•	Cấu hình đường dẫn chỉ mục, bộ lọc mở rộng, tùy chọn cập nhật chỉ mục định kỳ.
•	Cài đặt và tinh chỉnh (Settings) qua form cấu hình.
Các công cụ & thư viện chính (vai trò)
Dưới đây là danh sách packages chính dùng trong project và vai trò/ý nghĩa chính của một số thư viện quan trọng:
•	Lucene.Net (4.8.0-beta00014), Lucene.Net.Analysis.Common, Lucene.Net.QueryParser, Lucene.Net.Queries, Lucene.Net.Sandbox
•	Dùng làm engine lập chỉ mục và tra cứu full-text.
•	J2N (2.0.0-beta-0012)
•	Hỗ trợ chuyển đổi API giữa Java và .NET cho Lucene.Net.
•	PdfPig (0.1.16)
•	Trích xuất văn bản từ file PDF.
•	SautinSoft.Document (2026.8.27)
•	Đọc/chuyển đổi nhiều định dạng văn bản (DOC, DOCX, RTF, ODT, v.v.).
•	DotNetCore.NPOI (1.2.3) và các phụ thuộc OpenXml
•	Đọc/ghi file Excel (XLS/XLSX).
•	ExcelDataReader (3.9.0) & ExcelDataReader.DataSet
•	Đọc dữ liệu Excel.
•	HtmlAgilityPack (1.13.0)
•	Phân tích HTML, trích xuất text từ trang HTML.
•	SharpZipLib (1.4.2)
•	Giải nén/đọc file nén khi cần.
•	Newtonsoft.Json (13.0.3)
•	Đọc/ghi cấu hình JSON, serialize/deserialize.
•	SunnyUI (3.9.8) & SunnyUI.Common
•	Library UI/controls tùy biến cho WinForms.
•	SkiaSharp (3.119.2) và SkiaSharp.HarfBuzz
•	Xử lý đồ họa, rendering khi cần (SVG, hình ảnh).
•	Svg.* (3.2.1)
•	Xử lý và vẽ SVG.
•	NCalc (6.4.0)
•	Đánh giá biểu thức (nếu có tính năng bộ lọc nâng cao bằng biểu thức).
•	RestSharp (114.0.0)
•	Gọi API nếu cần đồng bộ hoặc cập nhật từ dịch vụ ngoài.
•	BouncyCastle / Portable.BouncyCastle / Pkcs11Interop
•	Mã hóa/chứng thực nếu cần xử lý tài liệu ký số hoặc mã hóa.
•	Microsoft.IO.RecyclableMemoryStream
•	Quản lý bộ nhớ stream hiệu quả khi xử lý tập tin lớn.
•	Microsoft.Extensions.* (DependencyInjection/Logging Abstractions)
•	Hỗ trợ cấu trúc, logging trừu tượng (nếu áp dụng).
•	ApiLibs (1.43.0)
•	Thư viện nội bộ/bên thứ ba (project reference) — kiểm tra mã để biết chi tiết dùng cho API gì.
•	Các thư viện hệ thống (System.*) và NETStandard.Library để tương thích.
Hướng dẫn cài đặt & chạy
Yêu cầu:
•	Windows 10/11 hoặc tương đương.
•	.NET Framework 4.7.2 đã cài.
Cài đặt:
1.	Chạy file setup.exe.
Hướng dẫn sử dụng cơ bản
1.	Mở Settings (Cài đặt) để cấu hình
	-Bộ lọc tập tin (mở rộng, loại bỏ)
	-Thư mục loại trừ
2	Chọn đường dẫn ổ đĩa/thư mục cần quét.
3.	Thực hiện "Index" (Lập chỉ mục) — ứng dụng sẽ quét các tệp theo cấu hình và tạo chỉ mục Lucene.
1.	Nhập từ khóa vào ô tìm kiếm và nhấn tìm:
•	Hỗ trợ tìm theo từ khóa: tên file và tìm kiếm trong nội dung toàn văn.
5.	Mở file từ kết quả hoặc mở hiển thị vị trí của file, xóa file.
Cấu hình nâng cao
•	Nếu cần thay đổi cách trích xuất nội dung kiểm tra LuceneSearchService.cs và logic đọc tập tin.
•	Thông số index (analyzer, tokenizer) điều chỉnh trong lớp xử lý Lucene (LuceneSearchService.cs).
Sửa lỗi thường gặp
•	Ứng dụng không khởi động: kiểm tra .NET Framework 4.7.2 đã cài hay chưa.
•	Không thể đọc một số file: kiểm tra quyền truy cập file và format file có được hỗ trợ bởi thư viện (SautinSoft / PdfPig / NPOI).
•	Hiệu năng lập chỉ mục chậm: giảm phạm vi quét, tăng bộ lọc tập tin, hoặc kiểm tra cấu hình bộ nhớ/IO.
Bảo mật & Giấy phép
•	Dự án trong repository được cấp phép MIT (theo yêu cầu trước đó). Kiểm tra file LICENSE trong repo để xác nhận nội dung.
•	Một số thư viện bên thứ ba (ví dụ SautinSoft) có thể có điều khoản thương mại riêng — kiểm tra giấy phép của từng thư viện nếu dùng trong môi trường thương mại.
Tài liệu & Mở rộng
•	Để thêm định dạng mới: thêm thư viện trích xuất text tương ứng và cập nhật logic đọc file (LuceneSearchService.cs).
•	Để bật logging chi tiết: tích hợp logger (Microsoft.Extensions.Logging hoặc custom logger) và bật mức log debug.
