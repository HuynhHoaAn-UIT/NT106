# Task 03+04 — Socket Server và xử lý nhiều client

## Phạm vi và đầu ra

Socket Server tạo TCP listener, bind địa chỉ và cổng, lắng nghe rồi nhận kết nối,cho phép nhiều client giữ kết nối TCP cùng lúc. Vòng lặp nhận kết nối tiếp tục chạy trong khi mỗi client có tác vụ đọc riêng. Khi một client ngắt, server dọn kết nối đó và vẫn phục vụ các client khác.


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



## Bàn giao cho Loan Kết nối và tự kết nối lại Socket Client

-, Loan đã có màn hình Chat MessageView.xaml. Việc tiếp theo là thêm Socket Client vào ứng dụng WPF để màn hình Chat có thể kết nối tới server của Ân.Trong project WPF, dùng TcpClient.ConnectAsync để kết nối khi mở màn hình Chat. Khi server hiện Accepted, phần kết nối ban đầu đã hoạt động. Mở hai phiên WPF để kiểm tra cả hai cùng kết nối được.`.
- Mở hai cửa sổ WPF và kiểm tra server ghi `Accepted` cho cả hai. Dừng rồi chạy lại server để kiểm tra WPF tự kết nối lại mà giao diện vẫn phản hồi. Mã server này đủ để thử kết nối và kết nối lại. Nút **Gửi** sẽ nối ở task chat tiếp theo.


## Chạy bản tích hợp Task 05 và 06

Từ thư mục gốc repo, chạy `dotnet run --project GlobalSuccess.SocketServer`. Khi mở `MessageView`, WPF gọi `await client.ConnectAsync("127.0.0.1", 8085)` trên cùng máy. Mở hai phiên WPF và xem server ghi `Accepted` cho cả hai.

Sau khi kết nối, WPF gửi từng frame theo Task 01. Bốn byte đầu là độ dài JSON UTF-8 theo big-endian. WPF đọc đúng số byte của mỗi frame và giữ vòng đọc chạy trong suốt phiên chat.

```json
{"header":{"version":1,"type":"CHAT_JOIN","requestId":"1"},"body":{"roomId":"hoc-bong-1"}}
{"header":{"version":1,"type":"CHAT_SEND","requestId":"2"},"body":{"roomId":"hoc-bong-1","text":"Chào mọi người!"}}
```

Server trả `CHAT_JOIN_RESULT` cho client vừa vào phòng. Khi gửi tin, cả người gửi và các client trong cùng phòng nhận `CHAT_MESSAGE` với `body.roomId` và `body.text`. Thử cho hai client vào cùng phòng, gửi từ A và xem B nhận tin. Sau đó cho B sang phòng khác và kiểm tra B không nhận tin của phòng cũ. WPF cần gửi lại `CHAT_JOIN` sau khi kết nối lại.
