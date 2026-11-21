# Task 03 — Lập trình Socket Server TCP nền tảng bằng C#

**Phụ trách:** Hòa Ân  
**Trạng thái:** Đang thực hiện. Đánh dấu hoàn thành sau khi chạy kiểm thử kết nối.  
**Đầu ra:** Mã khởi động TCP listener cho .NET 10, cách kiểm thử độc lập và quy ước để tích hợp WPF với API xác thực.

## Phạm vi của ứng dụng

| Chức năng | Tích hợp dự kiến |
| --- | --- |
| Đăng ký, đăng nhập và quên mật khẩu | WPF gọi ASP.NET Core API của Bảo. Giữ luồng OTP và JWT nhóm đã demo. |
| Đăng xuất | WPF xóa JWT khỏi phiên cục bộ và đóng kết nối chat khi tích hợp. |
| Hồ sơ cá nhân và xóa tài khoản | WPF gọi API. Khi xóa tài khoản thành công, cần thống nhất cách vô hiệu hóa quyền truy cập và đóng phiên chat đang mở. |
| Khám phá — lọc và theo dõi học bổng | WPF gọi API lọc và lưu lượt theo dõi khi các endpoint tương ứng sẵn sàng. |
| Cộng đồng + Chat (Discover & Chat) | Socket TCP phục vụ chat realtime. Phòng chat, định tuyến, broadcast, truyền file và các thao tác cộng đồng được triển khai theo các task tương ứng. |

Task 03 chỉ tạo socket, bind địa chỉ/cổng, listen, accept và ghi log kết nối. Ân có thể kiểm thử bằng một TCP client độc lập, trước khi ghép với WPF, JWT hay MySQL.

## Mã C# nền tảng

Từ thư mục làm việc, tạo project console .NET 10 nếu chưa có:

```powershell
dotnet new console -n GlobalSuccess.SocketServer -f net10.0
```

Thay nội dung `GlobalSuccess.SocketServer/Program.cs` bằng:

```csharp
using System.Net;
using System.Net.Sockets;

const int port = 8085;
using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

var listener = new TcpListener(IPAddress.Loopback, port);
try
{
    listener.Start();
    Console.WriteLine($"Listening on {listener.LocalEndpoint}");

    while (!shutdown.IsCancellationRequested)
    {
        using TcpClient client = await listener.AcceptTcpClientAsync(shutdown.Token);
        Console.WriteLine($"Accepted {client.Client.RemoteEndPoint}");
    }
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    Console.WriteLine("Server stopped.");
}
finally
{
    listener.Stop();
}
```

`TcpListener` tạo socket TCP. `Start()` bind và listen. `AcceptTcpClientAsync()` nhận kết nối. Bản nền tảng ghi log rồi đóng từng kết nối, vì vậy vòng lặp tiếp tục nhận client kế tiếp. Task xử lý nhiều client sẽ bổ sung vòng đời phiên và xử lý dữ liệu trên mỗi kết nối.

## Chạy thử và nghiệm thu

Terminal 1:

```powershell
dotnet run --project GlobalSuccess.SocketServer
```

Terminal 2 trên cùng máy:

```powershell
Test-NetConnection 127.0.0.1 -Port 8085
```

Đạt khi terminal server in `Listening on 127.0.0.1:8085`, client báo `TcpTestSucceeded : True`, server in `Accepted` kèm endpoint của client, lần chạy thứ hai được nhận và Ctrl+C dừng listener. Cổng TCP 8085 dùng cho kiểm thử Socket Server, tách khỏi cổng HTTP 5116 của API.

## Bàn giao để tích hợp

| Người | Phần sử dụng và cách tích hợp |
| --- | --- |
| **Ân** | Chạy listener và kiểm tra log. Khi làm task chat tiếp theo, áp dụng framing đã đề xuất ở [Task 01](task-01-socket-protocol.md): 4 byte độ dài big-endian, rồi JSON UTF-8 gồm `header` và `body`. |
| **Loan** | WPF có thể dùng `TcpClient.ConnectAsync` với `127.0.0.1:8085` để test kết nối trên cùng máy. Bản này đóng socket sau khi accept, nên chỉ dùng để kiểm tra kết nối. Khi Ân bổ sung phiên chat, dùng cùng framing và chốt các trường dữ liệu cho `CHAT_JOIN`, `CHAT_SEND`, `CHAT_MESSAGE` trước khi nối UI. |
| **Bảo** | Giữ API xác thực hiện tại. Trước khi gửi JWT qua kết nối chat thật, thống nhất TLS, cách Socket Server kiểm tra chữ ký, issuer, audience, thời hạn token và cách xử lý khi tài khoản bị xóa. Hai dịch vụ cấu hình riêng, dùng biến môi trường hoặc kho bí mật cho khóa và thông tin kết nối. |

Các tên gói tin ở phần bàn giao mô tả bước tích hợp sau Task 03. Mã C# ở trên thực hiện phần listener nền tảng.
