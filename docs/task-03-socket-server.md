# Task 03 — Lập trình Socket Server TCP nền tảng bằng C#


## Phạm vi của ứng dụng

| Chức năng | Tích hợp dự kiến |
| --- | --- |
| Đăng ký, đăng nhập và quên mật khẩu | WPF gọi ASP.NET Core API của Bảo. Giữ luồng OTP và JWT nhóm đã demo. |
| Đăng xuất | WPF xóa JWT khỏi phiên cục bộ và đóng kết nối chat khi tích hợp. |
| Hồ sơ cá nhân và xóa tài khoản | WPF gọi API. Khi xóa tài khoản thành công, cần thống nhất cách vô hiệu hóa quyền truy cập và đóng phiên chat đang mở. |
| Khám phá — lọc và theo dõi học bổng | WPF gọi API lọc và lưu lượt theo dõi khi các endpoint tương ứng sẵn sàng. |
| Cộng đồng + Chat (Discover & Chat) | Socket TCP phục vụ chat realtime. Phòng chat, định tuyến, broadcast, truyền file và các thao tác cộng đồng được triển khai theo các task tương ứng. |

Task 03 chỉ tạo socket, bind địa chỉ/cổng, listen, accept và ghi log kết nối.

## Mã C# nền tảng


`GlobalSuccess.SocketServer/Program.cs` :

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

## Bàn giao để tích hợp
Loan sẽ nối màn hình Chat WPF với Socket Server của Ân. Trước mắt, Loan dùng code Task 03 để thử xem ứng dụng có kết nối được tới server không. Khi Ân làm xong phần gửi và nhận tin nhắn, Loan sẽ nối nút Gửi và hiển thị tin nhắn nhận được.
Bảo đã làm API đăng nhập trả về JWT. Khi tích hợp chat, Bảo sẽ phối hợp với Socket Server kiểm tra JWT đó và biết ai đang kết nối. Bước thử kết nối hiện tại có thể chạy ngay, chưa cần JWT.
