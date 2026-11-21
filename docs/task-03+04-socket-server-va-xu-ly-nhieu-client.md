# Task 03+04 — Socket Server và xử lý nhiều client

**Phụ trách:** Hòa Ân  
**Kế hoạch:** 19/10/2026 – 23/10/2026  
**Triển khai:** Ân có thể làm và kiểm thử độc lập với WPF của Loan và Web API của Bảo.

## Phạm vi và đầu ra

Socket Server tạo TCP listener, bind địa chỉ và cổng, lắng nghe rồi nhận kết nối. Task 04 bao gồm các bước nền tảng này và cho phép nhiều client giữ kết nối TCP cùng lúc. Vòng lặp nhận kết nối tiếp tục chạy trong khi mỗi client có tác vụ đọc riêng. Khi một client ngắt, server dọn kết nối đó và vẫn phục vụ các client khác.

Đầu ra là mã C# cho `GlobalSuccess.SocketServer/Program.cs` và kịch bản kiểm thử trên cổng `8085`. Phần đọc dữ liệu hiện dùng để xác nhận từng kết nối hoạt động. Định dạng gói tin, phòng chat và broadcast thuộc các bước tiếp theo.

| Socket option | Cấu hình và mục đích |
| --- | --- |
| `NoDelay` | Bật trên từng client được nhận để tắt thuật toán Nagle, phù hợp khi server bắt đầu gửi các gói chat nhỏ. |
| `KeepAlive` | Bật TCP keepalive theo thiết lập hệ điều hành để hỗ trợ phát hiện kết nối rỗi đã mất. Heartbeat và tự kết nối lại thuộc phần Socket Client của Loan. |

## Mã C# (.NET 10)

Nếu chưa có project, tạo project console bằng `dotnet new console -n GlobalSuccess.SocketServer -f net10.0`. Sau đó dùng mã dưới đây cho `GlobalSuccess.SocketServer/Program.cs`:

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
var clientTasks = new List<Task>();

try
{
    listener.Start();
    Console.WriteLine($"Listening on {listener.LocalEndpoint}");

    while (!shutdown.IsCancellationRequested)
    {
        TcpClient client = await listener.AcceptTcpClientAsync(shutdown.Token);
        clientTasks.RemoveAll(task => task.IsCompleted);
        clientTasks.Add(HandleClientAsync(client, shutdown.Token));
    }
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    // Ctrl+C yêu cầu dừng server.
}
finally
{
    listener.Stop();
    shutdown.Cancel();
    await Task.WhenAll(clientTasks);
}

static async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
{
    using (client)
    {
        string? endpoint = client.Client.RemoteEndPoint?.ToString();

        try
        {
            client.NoDelay = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            Console.WriteLine($"Accepted {endpoint}");

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(), cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                Console.WriteLine($"Received {bytesRead} bytes from {endpoint}");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Server đang dừng.
        }
        catch (SocketException error)
        {
            Console.WriteLine($"Socket error for {endpoint}: {error.Message}");
        }
        catch (IOException error)
        {
            Console.WriteLine($"Connection error for {endpoint}: {error.Message}");
        }
        finally
        {
            Console.WriteLine($"Disconnected {endpoint}");
        }
    }
}
```

## Kiểm thử và nghiệm thu

Chạy server từ thư mục chứa project:

```powershell
dotnet run --project GlobalSuccess.SocketServer
```

Trong một cửa sổ PowerShell khác trên cùng máy, mở hai kết nối, đóng A rồi gửi thêm dữ liệu từ B:

```powershell
$clientA = [System.Net.Sockets.TcpClient]::new('127.0.0.1', 8085)
$clientB = [System.Net.Sockets.TcpClient]::new('127.0.0.1', 8085)
$clientA.Client.LocalEndPoint
$clientB.Client.LocalEndPoint
$payload = [System.Text.Encoding]::UTF8.GetBytes('test')
$clientA.GetStream().Write($payload, 0, $payload.Length)
$clientA.Dispose()
$clientB.GetStream().Write($payload, 0, $payload.Length)
$clientB.Dispose()
```

Đạt khi server ghi `Accepted` cho hai endpoint riêng, nhận dữ liệu của B sau khi A đóng, rồi ghi `Disconnected` cho mỗi kết nối. Thử thêm Ctrl+C khi client còn mở để kiểm tra server dừng và đóng các kết nối. Đánh dấu hoàn thành sau khi chạy kiểm thử.

## Bàn giao cho Loan — Task 3: Kết nối và tự kết nối lại Socket Client

- Chạy `GlobalSuccess.SocketServer` của Ân. Trong project `GlobalSuccess.Desktop` đã commit, thêm Socket Client bằng `TcpClient` và gọi `ConnectAsync` khi mở màn hình Chat (`MessageView`). Chạy WPF và server trên cùng máy để thử với `127.0.0.1:8085`.
- Mở hai cửa sổ WPF và kiểm tra server ghi `Accepted` cho cả hai. Dừng rồi chạy lại server để kiểm tra WPF tự kết nối lại mà giao diện vẫn phản hồi. Mã server này đủ để thử kết nối và kết nối lại. Nút **Gửi** sẽ nối ở task chat tiếp theo.
