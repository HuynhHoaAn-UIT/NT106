// Dùng chung header (version, type, requestId) và framing của Task 06.
// FILE_BEGIN body: { "fileName": "essay.pdf", "size": 123, "sha256": "<64 ký tự hex>" }
// FILE_CHUNK body: { "data": "<Base64 của tối đa 65536 byte>" }
// FILE_END body: {}
// Mỗi kết nối chỉ có một upload. Gửi khối theo thứ tự và đợi FILE_RESULT trước
// khi gửi gói tiếp theo. FILE_RESULT chứa stage, bytesReceived và fileId khi hoàn tất.
// ERROR báo upload thất bại. Sau khi kết nối lại, bắt đầu lại từ FILE_BEGIN.

internal static class FileProtocol
{
    public const string Begin = "FILE_BEGIN";
    public const string Chunk = "FILE_CHUNK";
    public const string End = "FILE_END";
    public const int MaxChunkSize = 64 * 1024;
    public const long MaxFileSize = 20 * 1024 * 1024;
}
