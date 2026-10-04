# 🔎 TimKiemNhanhTaiLieu

Ứng dụng **desktop trên Windows** phục vụ tìm kiếm nhanh tài liệu trên máy tính cục bộ. Ứng dụng sử dụng cơ chế **lập chỉ mục (Index) và tìm kiếm toàn văn (Full-text Search)** để trả về kết quả nhanh, đồng thời hỗ trợ nhiều định dạng tài liệu phổ biến như PDF, Word, Excel, HTML, văn bản thuần, v.v.

---

## 📖 Giới thiệu

**TimKiemNhanhTaiLieu** được xây dựng bằng **C# Windows Forms**, chạy trên **.NET Framework 4.7.2**.

Ứng dụng cho phép:

* 🔎 Tìm kiếm tài liệu nhanh trên máy tính.
* 📁 Quét và lập chỉ mục các thư mục/tập tin.
* 📝 Tìm kiếm theo **tên file**.
* 📄 Tìm kiếm theo **nội dung bên trong file**.
* ⚡ Sử dụng **Lucene.Net** để tối ưu tốc độ tìm kiếm.
* 📚 Hỗ trợ nhiều định dạng tài liệu.
* ⚙️ Cho phép cấu hình thư mục, bộ lọc và chỉ mục.
* 🖥️ Giao diện hiện đại sử dụng **SunnyUI**.

---

## 🖥️ Giao diện

Ứng dụng được phát triển dưới dạng **Windows Forms Application**.

Các Form chính:

* `Form1.cs` – giao diện tìm kiếm và hiển thị kết quả.
* `Settingsform.cs` – giao diện cấu hình ứng dụng.

Ứng dụng sử dụng thư viện **SunnyUI** để xây dựng các thành phần giao diện hiện đại và trực quan.

---

## 🚀 Tính năng chính

### 🔹 1. Lập chỉ mục tài liệu

Cho phép quét các thư mục hoặc ổ đĩa trên máy tính và xây dựng **Lucene Index**.

Quá trình lập chỉ mục bao gồm:

1. Xác định thư mục cần quét.
2. Lọc các loại file được hỗ trợ.
3. Đọc và trích xuất nội dung file.
4. Tạo dữ liệu chỉ mục.
5. Lưu chỉ mục để phục vụ tìm kiếm.

### 🔹 2. Tìm kiếm tài liệu

Hỗ trợ tìm kiếm theo:

* Tên file.
* Nội dung file.
* Từ khóa.
* Full-text search.

### 🔹 3. Hỗ trợ nhiều định dạng

Một số định dạng được hỗ trợ:

| Định dạng | Loại tài liệu     |
| --------- | ----------------- |
| `.pdf`    | PDF               |
| `.doc`    | Microsoft Word    |
| `.docx`   | Microsoft Word    |
| `.xls`    | Microsoft Excel   |
| `.xlsx`   | Microsoft Excel   |
| `.html`   | HTML              |
| `.htm`    | HTML              |
| `.txt`    | Văn bản thuần     |
| `.rtf`    | Rich Text Format  |
| `.odt`    | OpenDocument Text |

### 🔹 4. Quản lý kết quả tìm kiếm

Người dùng có thể:

* Mở trực tiếp file.
* Mở vị trí chứa file.
* Xem thông tin file.
* Xóa file.
* Tiếp tục thực hiện tìm kiếm với từ khóa khác.

### 🔹 5. Cấu hình ứng dụng

Thông qua **Settings**, người dùng có thể cấu hình:

* Đường dẫn thư mục cần quét.
* Thư mục loại trừ.
* Phần mở rộng file.
* Loại file cần bỏ qua.
* Đường dẫn lưu Index.
* Các tùy chọn cập nhật Index.

---

# 🛠️ Công cụ & thư viện

Dự án sử dụng nhiều thư viện hỗ trợ việc lập chỉ mục, tìm kiếm và xử lý tài liệu.

| Thư viện                       |       Phiên bản | Vai trò                                  |
| ------------------------------ | --------------: | ---------------------------------------- |
| **Lucene.Net**                 | 4.8.0-beta00014 | Engine lập chỉ mục và tìm kiếm Full-text |
| **Lucene.Net.Analysis.Common** | 4.8.0-beta00014 | Phân tích và xử lý văn bản               |
| **Lucene.Net.QueryParser**     | 4.8.0-beta00014 | Phân tích câu truy vấn tìm kiếm          |
| **Lucene.Net.Queries**         | 4.8.0-beta00014 | Hỗ trợ các truy vấn nâng cao             |
| **Lucene.Net.Sandbox**         | 4.8.0-beta00014 | Các chức năng mở rộng của Lucene         |
| **J2N**                        | 2.0.0-beta-0012 | Hỗ trợ API                               |
