// Chạy từ thư mục gốc: dotnet run --project GlobalSuccess.SocketServer.csproj
// Khi mở MessageView, WPF gọi await client.ConnectAsync("127.0.0.1", 8085).
// Mở hai phiên WPF trên cùng máy và kiểm tra server ghi Accepted cho cả hai.

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
var router = new ChatRouter();
var clientTasks = new List<Task>();

try
{
    listener.Start();
    Console.WriteLine($"Listening on {listener.LocalEndpoint}");

    while (!shutdown.IsCancellationRequested)
    {
        clientTasks.RemoveAll(task => task.IsCompletedSuccessfully);
        if (clientTasks.Count >= SocketLimits.MaxClients)
        {
            Task completedClient = await Task.WhenAny(clientTasks).WaitAsync(shutdown.Token);
            await completedClient;
            continue;
        }

        TcpClient client = await listener.AcceptTcpClientAsync(shutdown.Token);
        clientTasks.Add(router.HandleClientAsync(client, shutdown.Token));
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
