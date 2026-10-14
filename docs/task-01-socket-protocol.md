# Task 01 — Khảo sát và thiết kế giao thức Socket

## 1. Khảo sát branch `luuchibao7031`

Đã xem toàn bộ file trong branch. Đây là ứng dụng WinForms C# .NET 10, hiện mới có khung giao diện:

- **LoginForm:** ô email, mật khẩu; nút đăng nhập và đăng ký.
- **RegisterForm:** họ tên, email, mật khẩu, xác nhận mật khẩu.
- **OTPForm:** nhập OTP, xác thực và gửi lại mã.
- **MainForm:** hiện còn trống.
- Các file `.resx` chỉ là tài nguyên mặc định; chưa có chức năng nghiệp vụ.

Hiện chưa có Socket Client/Server, cơ sở dữ liệu hay xử lý thật cho đăng nhập, OTP và các chức năng sản phẩm. Trong code đăng nhập, biến email đang lấy nhầm từ ô mật khẩu; đăng ký vẫn mở màn OTP khi thông tin còn thiếu; xác thực OTP chấp nhận mọi mã không rỗng.

**Kết luận khảo sát:** branch này mới là phần khung đăng nhập/đăng ký. Giao thức cần được thiết kế từ yêu cầu sản phẩm bên dưới, rồi dùng chung cho Client và Server khi bắt đầu các task tiếp theo.

## 2. Đề xuất và phân tích giao thức Socket Header/Body

### 2.1 Mục tiêu

Giao thức cần phục vụ đủ 5 nhóm chức năng:

1. **Tài khoản:** đăng ký, đăng nhập, hồ sơ cá nhân, xóa tài khoản.
2. **Khám phá và chat:** lọc học bổng, theo dõi học bổng, chat nhóm thời gian thực.
3. **Cộng đồng:** bảng tin, upvote, bình luận, bookmark, báo cáo và lọc nội dung.
4. **Kho bài luận:** xem và tải tài liệu luyện viết miễn phí.
5. **Mentor và gamification:** mở khóa Mentor, hồ sơ Mentor, xếp hạng, Top 100, biểu đồ hoạt động, trợ giúp và khiếu nại.

### 2.2 Cách đóng gói một message

TCP chỉ chuyển một luồng byte; nó không báo ranh giới giữa các message. Vì vậy đề xuất mỗi lần gửi theo dạng:

**4 byte độ dài + nội dung JSON UTF-8**

Bốn byte đầu ghi độ dài phần JSON theo thứ tự byte mạng (big-endian). Sau đó bên nhận đọc đúng số byte đã ghi. Cách này đơn giản để làm bộ đọc TCP và xử lý được trường hợp dữ liệu đến thành nhiều phần.

Trong JSON, chia thông tin thành **header** và **body**:

- **header:** loại yêu cầu, phiên bản giao thức, mã yêu cầu và mã phiên đăng nhập.
- **body:** dữ liệu cụ thể của chức năng.

Ví dụ yêu cầu đăng nhập:

```json
{
  "header": {
    "version": 1,
    "type": "ACCOUNT_LOGIN",
    "requestId": "req-1001",
    "token": null
  },
  "body": {
    "email": "an@example.com",
    "password": "..."
  }
}
```

Ví dụ phản hồi thành công:

```json
{
  "header": {
    "version": 1,
    "type": "ACCOUNT_LOGIN_RESULT",
    "requestId": "req-1001"
  },
  "body": {
    "success": true,
    "userId": "user-25",
    "message": "Đăng nhập thành công"
  }
}
```

### 2.3 Các loại yêu cầu chính

| Nhóm chức năng | Loại message đề xuất |
|---|---|
| Tài khoản | ACCOUNT_REGISTER, ACCOUNT_LOGIN, ACCOUNT_GET_PROFILE, ACCOUNT_UPDATE_PROFILE, ACCOUNT_DELETE |
| OTP | OTP_VERIFY, OTP_RESEND |
| Học bổng | SCHOLARSHIP_SEARCH, SCHOLARSHIP_FOLLOW |
| Chat | CHAT_JOIN, CHAT_SEND, CHAT_MESSAGE |
| Cộng đồng | FEED_GET, POST_CREATE, POST_UPVOTE, POST_COMMENT, POST_BOOKMARK, POST_REPORT |
| Lọc nội dung | CONTENT_FILTER_RESULT |
| Kho bài luận | WRITING_RESOURCE_LIST, WRITING_RESOURCE_DOWNLOAD |
| Mentor | MENTOR_UNLOCK, MENTOR_PROFILE_SAVE, MENTOR_RANK_GET |
| Gamification | LEADERBOARD_TOP100, ACTIVITY_HEATMAP_GET |
| Trợ giúp | SUPPORT_CREATE, SUPPORT_LIST, SUPPORT_REPLY |
| Lỗi | ERROR |

Các tên trên là mã kỹ thuật để Client và Server dùng thống nhất. Mỗi chức năng sẽ chốt chi tiết trường dữ liệu khi lập trình.

### 2.4 Quy tắc gửi và nhận

- Mỗi yêu cầu có một `requestId`; phản hồi trả lại cùng mã đó để Client biết kết quả thuộc thao tác nào.
- Tin nhắn chat do Server phát tới thành viên trong phòng là message chủ động, không cần chờ một yêu cầu tương ứng.
- Body là JSON UTF-8; giới hạn ban đầu đề xuất 1 MiB cho mỗi message.
- Dữ liệu tệp lớn phải chia thành các phần nhỏ; không gửi cả tệp trong một message JSON.
- Server kiểm tra đăng nhập và quyền truy cập trước khi xử lý yêu cầu.
- Client gửi heartbeat định kỳ; nếu mất kết nối thì đóng Socket và thực hiện kết nối lại.
- Dữ liệu truyền qua mạng cần được bảo vệ; việc dùng TLS/AES và cách cấp khóa phải được thống nhất trước khi triển khai.

### 2.5 Vì sao chọn cách này?

Cách đóng gói có độ dài cố định ở đầu message dễ hiểu và dễ kiểm tra hơn việc đoán ranh giới message trong TCP. Header JSON tách riêng loại yêu cầu và mã đối chiếu khỏi dữ liệu chức năng, nên có thể bổ sung tính năng mà không phải thiết kế lại toàn bộ luồng truyền.

Đây là đề xuất cho Task 01, chưa phải code. Trước khi lập trình, nhóm cần thống nhất địa chỉ/cổng Server, cấu trúc dữ liệu từng chức năng và cách xác thực phiên đăng nhập.
