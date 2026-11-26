using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace GlobalSuccess.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Bắt buộc phải có JWT Token mới được truy cập
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;

        public UserController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:DefaultConnection trong appsettings.json");
        }

        // 1. API XEM HỒ SƠ: GET /api/user/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Trích xuất UserId từ Token JWT của người dùng đang gửi request
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
                return Unauthorized("Token không hợp lệ hoặc không xác định được người dùng.");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                SELECT 
                    user_id AS UserId, 
                    email AS Email, 
                    full_name AS FullName, 
                    avatar_url AS AvatarUrl, 
                    created_at AS CreatedAt 
                FROM Users 
                WHERE user_id = @UserId 
                LIMIT 1";

            var user = await connection.QuerySingleOrDefaultAsync<UserProfileResponse>(sql, new { UserId = userId });

            if (user == null)
                return NotFound("Không tìm thấy thông tin người dùng trong hệ thống.");

            return Ok(user);
        }

        // 2. API CẬP NHẬT HỒ SƠ: PUT /api/user/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            // Trích xuất UserId từ Token JWT
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
                return Unauthorized("Token không hợp lệ.");

            if (string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest("Họ và tên không được để trống.");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                UPDATE Users 
                SET full_name = @FullName, 
                    avatar_url = @AvatarUrl,
                    updated_at = NOW() 
                WHERE user_id = @UserId";

            int rowsAffected = await connection.ExecuteAsync(sql, new
            {
                FullName = request.FullName.Trim(),
                AvatarUrl = request.AvatarUrl?.Trim(),
                UserId = userId
            });

            if (rowsAffected == 0)
                return NotFound("Không thể cập nhật hồ sơ, người dùng không tồn tại.");

            return Ok(new { message = "Cập nhật hồ sơ thành công!" });
        }

        // 3. API TẢI LÊN ẢNH ĐẠI DIỆN: POST /api/user/upload-avatar
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile? file)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
                return Unauthorized("Token không hợp lệ.");

            file ??= Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn một file ảnh hợp lệ.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Dung lượng ảnh không được vượt quá 5MB.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Định dạng file không được hỗ trợ. Vui lòng chọn ảnh JPG, PNG hoặc WEBP.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"avatar_{userId}_{DateTime.UtcNow.Ticks}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string avatarUrl = $"{Request.Scheme}://{Request.Host}/uploads/avatars/{uniqueFileName}";
            return Ok(new { avatarUrl });
        }
    }

    // DTO (Data Transfer Object) dùng để trả về thông tin hồ sơ an toàn (không lộ password)
    public class UserProfileResponse
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO nhận dữ liệu gửi lên khi muốn cập nhật
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
