using System.Security.Cryptography;
using System.Text.Json;

internal sealed partial class FileUpload : IAsyncDisposable
{
    private FileStream? output;
    private IncrementalHash? hash;
    private string? temporaryPath;
    private string? fileId;
    private string? originalName;
    private byte[]? expectedHash;
    private long expectedSize;
    private long received;

    public bool Active => output is not null;

    public async Task<object> HandleAsync(string type, JsonElement body, CancellationToken cancellationToken)
    {
        if (type == FileProtocol.Begin)
        {
            if (Active)
            {
                throw new InvalidOperationException();
            }

            var metadata = ReadMetadata(body);
            string folder = Path.Combine(AppContext.BaseDirectory, "uploads");
            Directory.CreateDirectory(folder);
            fileId = Guid.NewGuid().ToString("N");
            string path = Path.Combine(folder, fileId + ".part");
            output = new FileStream(path, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, 1, FileOptions.Asynchronous);
            temporaryPath = path;
            originalName = metadata.Name;
            expectedSize = metadata.Size;
            expectedHash = metadata.Hash;
            hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            received = 0;
            return new { stage = "begin", bytesReceived = received };
        }

        FileStream stream = output ?? throw new InvalidOperationException();
        if (type == FileProtocol.Chunk)
        {
            byte[] data = ReadChunk(body, expectedSize - received);
            await stream.WriteAsync(data, cancellationToken);
            hash!.AppendData(data);
            received += data.Length;
            return new { stage = "chunk", bytesReceived = received };
        }

        if (received != expectedSize ||
            !CryptographicOperations.FixedTimeEquals(hash!.GetHashAndReset(), expectedHash!))
        {
            throw new InvalidDataException("File size or SHA-256 does not match.");
        }

        await stream.DisposeAsync();
        output = null;
        cancellationToken.ThrowIfCancellationRequested();
        File.Move(temporaryPath!, Path.ChangeExtension(temporaryPath!, ".upload"));
        temporaryPath = null;
        hash.Dispose();
        hash = null;
        return new { stage = "end", bytesReceived = received, fileId, fileName = originalName };
    }
}
