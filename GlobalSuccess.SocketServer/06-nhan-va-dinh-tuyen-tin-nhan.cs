using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text.Json;

internal sealed class ChatRouter
{
    private const int MaxFrameSize = 1024 * 1024;
    private readonly ChatRooms rooms = new();

    public async Task HandleClientAsync(TcpClient socket, CancellationToken shutdown)
    {
        using (socket)
        using (var session = CancellationTokenSource.CreateLinkedTokenSource(shutdown))
        {
            string? endpoint = null;
            var client = new ChatClient(socket);
            Task writer = Task.CompletedTask;

            try
            {
                endpoint = socket.Client.RemoteEndPoint?.ToString();
                socket.NoDelay = true;
                socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                NetworkStream stream = socket.GetStream();
                Console.WriteLine($"Accepted {endpoint}");
                writer = client.WriteAsync(stream, session.Token);

                byte[] length = new byte[sizeof(int)];
                while (await stream.ReadAsync(length.AsMemory(0, 1), session.Token) != 0)
                {
                    await stream.ReadExactlyAsync(length.AsMemory(1), session.Token);
                    int size = BinaryPrimitives.ReadInt32BigEndian(length);
                    if (size is <= 0 or > MaxFrameSize)
                    {
                        throw new InvalidDataException("Invalid frame length.");
                    }

                    byte[] json = new byte[size];
                    await stream.ReadExactlyAsync(json, session.Token);
                    Route(client, json);
                }
            }
            catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
            {
                // Server đang dừng.
            }
            catch (ObjectDisposedException)
            {
                // Socket đã đóng.
            }
            catch (Exception error) when (error is IOException or SocketException)
            {
                Console.WriteLine($"Connection error for {endpoint}: {error.Message}");
            }
            finally
            {
                rooms.Leave(client);
                session.Cancel();
                socket.Dispose();
                await writer;
                Console.WriteLine($"Disconnected {endpoint}");
            }
        }
    }

    private void Route(ChatClient client, byte[] json)
    {
        string? requestId = null;

        try
        {
            using var packet = JsonDocument.Parse(json);
            JsonElement header = packet.RootElement.GetProperty("header");
            requestId = header.GetProperty("requestId").GetString();
            if (header.GetProperty("version").GetInt32() != 1 ||
                string.IsNullOrWhiteSpace(requestId) || requestId.Length > 64)
            {
                throw new FormatException();
            }

            string? type = header.GetProperty("type").GetString();
            JsonElement body = packet.RootElement.GetProperty("body");
            string? roomId = body.GetProperty("roomId").GetString();
            if (string.IsNullOrWhiteSpace(roomId) || roomId.Length > 64)
            {
                throw new FormatException();
            }

            switch (type)
            {
                case "CHAT_JOIN":
                    rooms.Join(client, roomId, Frame("CHAT_JOIN_RESULT", requestId, new { roomId }));
                    return;
                case "CHAT_SEND":
                    string? text = body.GetProperty("text").GetString();
                    if (string.IsNullOrWhiteSpace(text) || text.Length > 4096)
                    {
                        throw new FormatException();
                    }

                    if (!rooms.Broadcast(client, roomId, Frame("CHAT_MESSAGE", null, new { roomId, text })))
                    {
                        client.Send(Frame("ERROR", requestId, new { message = "Join the room first." }));
                    }

                    return;
                default:
                    throw new FormatException();
            }
        }
        catch (Exception error) when (error is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            if (requestId?.Length > 64)
            {
                requestId = null;
            }

            client.Send(Frame("ERROR", requestId, new { message = "Invalid chat request." }));
        }
    }

    private static byte[] Frame(string type, string? requestId, object body)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new
        {
            header = new { version = 1, type, requestId },
            body
        });

        byte[] frame = new byte[sizeof(int) + json.Length];
        BinaryPrimitives.WriteInt32BigEndian(frame, json.Length);
        json.CopyTo(frame, sizeof(int));
        return frame;
    }
}
