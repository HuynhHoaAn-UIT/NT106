# Task 01 — Khảo sát yêu cầu và thiết kế giao thức Socket

**Dự án:** Scholarship Gateway  
**Nhánh GitHub:** huynhhoa-an  
**Trạng thái:** Tài liệu thiết kế để nhóm dùng chung khi viết Client và Server

## 1. Repo hiện có gì?

Nhánh này hiện có ứng dụng WinForms C# với các màn hình đăng nhập, đăng ký, nhập mã OTP và màn hình chính. Khi bấm nút, chương trình mới hiện thông báo; chưa có kết nối mạng, Socket Server hay cơ sở dữ liệu.

Tài liệu này thống nhất cách Client và Server gửi dữ liệu cho nhau. Task 01 chỉ thiết kế giao thức, chưa viết Socket Server.

## 2. Giao thức Socket là gì?

Có thể hình dung mỗi lần gửi dữ liệu giống như gửi một gói hàng:

- **Header (phần đầu):** ghi loại gói tin, độ dài dữ liệu và mã để ghép yêu cầu với phản hồi.
- **Body (phần nội dung):** chứa dữ liệu thật, thường là JSON.
- **TCP:** chuyển các byte giữa hai máy. TCP không giữ ranh giới từng gói tin ứng dụng, nên một lần nhận có thể chỉ lấy được một phần message, hoặc nhận liền nhiều message.

Vì vậy, Client và Server phải đọc theo độ dài ghi trong Header, không được nghĩ rằng một lần nhận dữ liệu sẽ luôn là một message hoàn chỉnh.

## 3. Những chức năng cần trao đổi dữ liệu

Theo danh sách task của dự án, giao thức cần hỗ trợ:

- Đăng ký, xác minh OTP, đăng nhập và đăng xuất.
- Kiểm tra kết nối định kỳ bằng heartbeat.
- Lấy danh sách học bổng và theo dõi học bổng.
- Gửi và nhận tin nhắn trong phòng chat của từng học bổng.
- Tạo bài viết và bình chọn.
- Lấy bảng xếp hạng Mentor.
- Gửi bài luận hoặc tệp theo từng phần nhỏ.
- Trả lỗi theo cách thống nhất để ứng dụng hiển thị cho người dùng.

Các chức năng này cần được nhóm xác nhận payload cụ thể trước khi bắt đầu lập trình.

## 4. Cấu trúc một message

Mỗi message gồm **Header 20 byte** và **Body** ngay sau Header. Các số dài hơn một byte được ghi theo thứ tự byte mạng (big-endian).

| Vị trí bắt đầu | Độ dài | Tên trường trong giao thức | Ý nghĩa dễ hiểu |
|---:|---:|---|---|
| 0 | 4 byte | Magic | Dấu nhận dạng, luôn là chữ SGW1 |
| 4 | 1 byte | Version | Phiên bản giao thức, hiện tại là 1 |
| 5 | 1 byte | Flags | Cho biết đây là phản hồi, lỗi, dữ liệu mã hóa hay dữ liệu nhị phân |
| 6 | 2 byte | MessageType | Mã cho biết đây là loại yêu cầu nào |
| 8 | 4 byte | BodyLength | Số byte của phần nội dung |
| 12 | 8 byte | RequestId | Mã để ghép phản hồi với yêu cầu ban đầu |

**BodyLength tính bằng byte, không phải số ký tự.** Sau khi đọc đủ 20 byte Header, bên nhận kiểm tra Header rồi đọc đúng số byte BodyLength.

### Giới hạn kích thước

- Nội dung JSON: tối đa 1 MiB.
- Mỗi phần dữ liệu của tệp: tối đa 64 KiB.
- Nếu dấu nhận dạng, phiên bản hoặc độ dài không hợp lệ, bên nhận dừng kết nối.
- Phải kiểm tra độ dài trước khi tạo vùng nhớ để chứa Body.

## 5. Nội dung Body

Hầu hết Body là JSON được mã hóa UTF-8. Tên trường JSON dùng kiểu camelCase như các ví dụ dưới đây; thời gian dùng UTC.

Yêu cầu đăng nhập:

```json
{
  "type": "LOGIN",
  "data": {
    "email": "student@example.edu",
    "password": "..."
  }
}
```

Phản hồi đăng nhập thành công:

```json
{
  "type": "LOGIN",
  "data": {
    "userId": "usr_123",
    "displayName": "Nguyen Van A",
    "token": "..."
  }
}
```

Phản hồi khi có lỗi:

```json
{
  "type": "ERROR",
  "data": {
    "code": "AUTH_INVALID_CREDENTIALS",
    "message": "Email hoặc mật khẩu không đúng.",
    "retryable": false
  }
}
```

Các từ như LOGIN, ERROR, email và password là tên trường/mã kỹ thuật để Client và Server dùng giống hệt nhau. Phần giải thích trong tài liệu được viết bằng tiếng Việt.

## 6. Mã loại message đề xuất

Các mã bên dưới là tên quy ước cho phiên bản giao thức 1. Client và Server phải dùng cùng mã và cùng cấu trúc dữ liệu.

| Mã | Hướng gửi | Dùng để làm gì |
|---|---|---|
| HELLO, HELLO_ACK | Client ↔ Server | Bắt đầu kết nối và xác nhận hai bên hiểu cùng phiên bản |
| ERROR | Hai chiều | Báo yêu cầu không hợp lệ hoặc thao tác thất bại |
| REGISTER | Client → Server | Gửi thông tin đăng ký |
| REGISTER_OTP_VERIFY | Client → Server | Gửi mã OTP để xác minh |
| REGISTER_OTP_RESEND | Client → Server | Yêu cầu gửi lại OTP |
| LOGIN, LOGOUT | Client → Server | Đăng nhập hoặc đăng xuất |
| PING, PONG | Client ↔ Server | Kiểm tra kết nối còn hoạt động |
| SCHOLARSHIP_LIST, SCHOLARSHIP_LIST_RESULT | Client ↔ Server | Yêu cầu và trả về danh sách học bổng |
| SCHOLARSHIP_FOLLOW | Client → Server | Theo dõi một học bổng |
| CHAT_SEND, CHAT_MESSAGE | Client ↔ Server | Gửi tin nhắn và phát tin tới thành viên cùng phòng |
| CHAT_HISTORY, CHAT_HISTORY_RESULT | Client ↔ Server | Yêu cầu và trả về lịch sử chat |
| POST_CREATE, POST_VOTE | Client → Server | Tạo bài viết và bình chọn |
| LEADERBOARD_TOP, LEADERBOARD_RESULT | Client ↔ Server | Lấy danh sách Mentor xếp hạng cao |
| LEADERBOARD_POSITION, LEADERBOARD_POSITION_RESULT | Client ↔ Server | Lấy thứ hạng và điểm của một Mentor |
| FILE_UPLOAD_START | Client → Server | Báo bắt đầu gửi tệp và thông tin tệp |
| FILE_CHUNK | Client → Server | Gửi một phần dữ liệu tệp |
| FILE_UPLOAD_COMPLETE, FILE_UPLOAD_RESULT | Client ↔ Server | Báo gửi xong và nhận kết quả kiểm tra |
| SESSION_REVOKED | Server → Client | Báo phiên đăng nhập đã bị thu hồi |

Tên trên là mã kỹ thuật, không phải câu tiếng Anh trong giao diện. Nhóm có thể điều chỉnh trước khi lập trình; sau khi Client và Server đã dùng, không đổi mã tùy tiện.

## 7. Cách ghép yêu cầu với phản hồi

Mỗi yêu cầu có một RequestId. Phản hồi phải trả lại đúng RequestId đó để Client biết nó ứng với thao tác nào. RequestId 0 dành cho tin nhắn Server tự gửi, ví dụ tin chat mới hoặc thông báo phiên đăng nhập bị thu hồi.

Trong Header, cờ response đánh dấu phản hồi. Nếu phản hồi báo lỗi thì bật thêm cờ error.

## 8. Luồng kết nối cơ bản

1. Client kết nối tới địa chỉ và cổng của Server.
2. Client gửi HELLO để báo phiên bản giao thức.
3. Server trả HELLO_ACK nếu hỗ trợ phiên bản đó. Nếu không, Server trả ERROR rồi đóng kết nối.
4. Client gửi LOGIN, hoặc gửi REGISTER rồi xác minh OTP.
5. Sau khi đăng nhập, Client gửi PING mỗi 15 giây; Server trả PONG.
6. Khi đăng xuất hoặc mất kết nối, Client dừng heartbeat và đóng Socket.
7. Khi kết nối lại, Client đăng nhập lại nếu phiên trước không còn hiệu lực.

Server phải tự kiểm tra quyền của người dùng với từng Scholarship_ID. Không được tin rằng người dùng có quyền chỉ vì Client gửi ID đó lên.

## 9. Mã hóa và gửi tệp

Task 26 có yêu cầu dùng AES. Không được ghi cùng một khóa AES cố định vào mã nguồn Client và Server. Nhóm cần thống nhất cách cấp khóa trước khi bật mã hóa. Nếu dùng AES, nên dùng AES-GCM và phải có cách kiểm tra dữ liệu không bị sửa.

AES ở tầng ứng dụng không thay thế việc bảo vệ kết nối mạng bằng TLS. Không ghi mật khẩu, OTP, token hoặc khóa mã hóa vào log.

Khi gửi tệp:

1. Client gửi FILE_UPLOAD_START kèm tên tệp, kích thước, mã kiểm tra SHA-256 và đối tượng liên quan.
2. Client chia tệp thành các phần, mỗi phần không quá 64 KiB, rồi gửi bằng FILE_CHUNK.
3. Client gửi FILE_UPLOAD_COMPLETE.
4. Server kiểm tra tổng kích thước và SHA-256 rồi mới báo thành công.

Nhóm cần thống nhất cách mỗi phần gắn với tệp nào, thứ tự phần, vị trí byte và việc có cho phép gửi tiếp sau khi mất kết nối hay không. Không nên chuyển tệp lớn thành một chuỗi base64 trong JSON.

## 10. Quy tắc khi lập trình Client và Server

- Đọc đủ 20 byte Header trước, sau đó mới đọc Body.
- Có thể phải đọc nhiều lần mới đủ dữ liệu; cũng có thể một lần đọc lấy được nhiều message.
- Kiểm tra độ dài và cờ trước khi đọc Body.
- Không để hai luồng gửi cùng ghi vào một Socket cùng lúc, vì dữ liệu có thể bị trộn.
- Giới hạn số dữ liệu chờ xử lý để tránh một Client chậm làm Server hết bộ nhớ.
- Cập nhật giao diện WinForms bằng Invoke hoặc BeginInvoke, không cập nhật giao diện trực tiếp từ luồng nhận dữ liệu.
- Khi mất kết nối, dừng việc đọc, gửi và heartbeat; sau đó đóng Socket và giải phóng bộ nhớ.
- Khi báo lỗi cho Client, chỉ gửi mã lỗi và nội dung dễ hiểu; không gửi stack trace hoặc thông tin nhạy cảm.

## 11. Việc nhóm cần thống nhất tiếp

1. Địa chỉ và cổng Server dùng khi phát triển và trình diễn.
2. Sau đăng nhập, Server trả token hay giữ phiên theo kết nối Socket?
3. OTP hết hạn sau bao lâu, được nhập sai bao nhiêu lần và gửi lại sau bao lâu?
4. Khóa AES được tạo và chuyển cho Client như thế nào?
5. Chat có gửi tệp đính kèm không?
6. Có cần tiếp tục gửi tệp từ phần đang dang dở sau khi kết nối lại không?
7. Danh sách học bổng, bài viết, hồ sơ Mentor và hỗ trợ dùng Socket hay một giao diện HTTP riêng?
8. Bảng xếp hạng tính riêng theo từng học bổng hay tính chung?

Các câu hỏi này không cản trở việc làm bộ đọc Header cơ bản. Tuy nhiên, cần trả lời trước khi chốt dữ liệu cụ thể cho từng chức năng.

## 12. Khi nào Task 01 được xem là xong?

- Nhóm biết cách đóng khung và đọc một message TCP.
- Các trường Header, giới hạn kích thước và cách báo lỗi đã được ghi rõ.
- Client và Server biết cách ghép phản hồi với yêu cầu.
- Heartbeat và tin nhắn Server chủ động đã được mô tả.
- Những điểm chưa thống nhất đã được liệt kê để nhóm quyết định.
- Chưa bao gồm code Socket Server hay thiết kế đầy đủ cơ sở dữ liệu.
