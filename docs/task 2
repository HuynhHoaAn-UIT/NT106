Task 02 — Thiết kế CSDL quan hệ (Database Design)
1. Users (Quản lý tài khoản)
Tên trường (Column) Kiểu dữ liệu Mô tả chi tiết
user_id INT / VARCHAR (PK) Mã định danh người dùng
email VARCHAR (Unique) Email đăng nhập
password_hash VARCHAR Mật khẩu mã hóa
full_name VARCHAR Họ và tên người dùng
avatar_url VARCHAR Đường dẫn ảnh đại diện
created_at TIMESTAMP Thời gian tạo tài khoản

2. Scholarships (Quản lý học bổng)
Tên trường (Column) Kiểu dữ liệu Mô tả chi tiết
scholarship_id INT / VARCHAR (PK) Mã học bổng
title VARCHAR Tiêu đề học bổng
organization VARCHAR Tổ chức/Trường cấp học

bổng
deadline DATETIME Hạn nộp hồ sơ
description TEXT Nội dung chi tiết/yêu cầu
created_at TIMESTAMP Thời gian tạo

3. Follows (Theo dõi học bổng)
Tên trường (Column) Kiểu dữ liệu Mô tả chi tiết
follow_id INT (PK, Auto) Mã theo dõi
user_id INT (FK -&gt; Users) Người theo dõi
scholarship_id INT (FK -&gt; Scholarships) Học bổng được theo dõi
followed_at TIMESTAMP Thời gian bấm theo dõi

4. ChatRooms (Phòng chat nhóm / Học bổng)
Tên trường (Column) Kiểu dữ liệu Mô tả chi tiết
room_id INT / VARCHAR (PK) Mã phòng chat
room_name VARCHAR Tên phòng chat
scholarship_id INT (FK, Nullable) Liên kết phòng chat với học

bổng
created_at TIMESTAMP Thời gian tạo

5. Messages (Tin nhắn chat thời gian thực)
Tên trường (Column) Kiểu dữ liệu Mô tả chi tiết
message_id INT / VARCHAR (PK) Mã tin nhắn
room_id INT (FK -&gt; ChatRooms) Phòng chat chứa tin nhắn
sender_id INT (FK -&gt; Users) Người gửi tin nhắn
content TEXT Nội dung tin nhắn văn bản
sent_at TIMESTAMP Thời gian gửi

6. Files (Quản lý file đính kèm / bài luận)
Tên trường (Column) Kiểu dữ liệu Mô tả chi tiết
file_id INT / VARCHAR (PK) Mã file
uploader_id INT (FK -&gt; Users) Người tải lên
file_name VARCHAR Tên gốc của file
file_size BIGINT Kích thước file (byte)
file_path VARCHAR Đường dẫn lưu trữ trên

server
uploaded_at TIMESTAMP Thời gian tải lên

7. ActivityLogs (Nhật ký hoạt động / Activity Heatmap)

Tên trường (Column) Kiểu dữ liệu Mô tả chi tiết
log_id INT (PK, Auto) Mã nhật ký
user_id INT (FK -&gt; Users) Người thực hiện hành động
activity_type VARCHAR Loại hành động (LOGIN,
CHAT, UPLOAD, FOLLOW...)
activity_date DATE Ngày diễn ra hành động
(dùng cho Heatmap)
created_at TIMESTAMP Thời gian chi tiết

Mối quan hệ giữa các bảng (Relationships)
- Users (1) -&gt; (N) Follows (N) &lt;- (1) Scholarships
- ChatRooms (1) -&gt; (N) Messages (N) &lt;- (1) Users
- Users (1) -&gt; (N) Files
- Users (1) -&gt; (N) ActivityLogs
