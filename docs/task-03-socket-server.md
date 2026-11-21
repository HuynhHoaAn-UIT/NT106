# Task 03 — Lập trình Socket Server TCP nền tảng bằng C#


## Task này đối với của ứng dụng

Cộng đồng + Chat (Discover & Chat)	Hỗ trợ trực tiếp phần Chat. Loan có thể thử kết nối WPF với server của Ân ngay. Các bước tiếp theo sẽ dùng kết nối này để vào phòng, gửi và nhận tin nhắn realtime, rồi truyền file.

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


```powershell
Test-NetConnection 127.0.0.1 -Port 8085
```

## Bàn giao để tích hợp
Loan —: Kết nối Socket Client từ WPF đến Server. Loan chạy code của Ân, rồi cho WPF kết nối thử. Server hiện Accepted là đạt phần kết nối ban đầu.
