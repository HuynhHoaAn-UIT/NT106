using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MySqlConnector;
using Dapper;
using GlobalSuccess.Api.Services;

namespace GlobalSuccess.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly JwtTokenService _jwtService;
        private readonly string _connectionString;

        public AuthController(IConfiguration config, JwtTokenService jwtService)
        {
            _config = config;
            _jwtService = jwtService;
            // Đọc chuỗi kết nối MySQL Azure từ appsettings.json
            _connectionString = _config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:DefaultConnection trong appsettings.json");
        }
        // 1. API ĐĂNG KÝ: POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email và Mật khẩu không được để trống.");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Kiểm tra email đã có trong bảng Users chưa
            string checkSql = "SELECT COUNT(1) FROM Users WHERE email = @Email";
            int exists = await connection.ExecuteScalarAsync<int>(checkSql, new { request.Email });
            if (exists > 0)
                return BadRequest("Email này đã được sử dụng. Vui lòng chọn email khác.");

            // Băm mật khẩu bằng BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Thêm người dùng mới vào bảng Users
            string insertSql = @"
                INSERT INTO Users (email, password_hash, full_name, created_at)
                VALUES (@Email, @PasswordHash, @FullName, NOW());
                SELECT LAST_INSERT_ID();";

            long newUserId = await connection.ExecuteScalarAsync<long>(insertSql, new
            {
                Email = request.Email.Trim(),
                PasswordHash = passwordHash,
                FullName = request.FullName.Trim()
            });

            return Ok(new { message = "Đăng ký tài khoản thành công!", userId = newUserId });
        }
        // 2. API ĐĂNG NHẬP: POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Vui lòng nhập Email và Mật khẩu.");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Tìm user theo Email trong MySQL Azure
            string querySql = "SELECT user_id, email, password_hash, full_name FROM Users WHERE email = @Email LIMIT 1";
            var user = await connection.QuerySingleOrDefaultAsync<dynamic>(querySql, new { request.Email });

            if (user == null)
                return Unauthorized("Email hoặc Mật khẩu không chính xác.");

            // So khớp mật khẩu với chuỗi hash trong database
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, (string)user.password_hash);
            if (!isPasswordCorrect)
                return Unauthorized("Email hoặc Mật khẩu không chính xác.");

            // Sinh chuỗi JWT Token (Task 26)
            string token = _jwtService.CreateToken((long)user.user_id, (string)user.email, (string)user.full_name);

            return Ok(new AuthResponse
            {
                Token = token,
                UserId = (long)user.user_id,
                Email = (string)user.email,
                FullName = (string)user.full_name
            });
        }
        // 3. API TEST BẢO MẬT BẰNG JWT
        [Authorize] // Bắt buộc phải có Bearer Token
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            return Ok(new { message = $"Xin chào {userEmail}, bạn đã gửi JWT Token chính xác!" });
        }
    }

    // KHAI BÁO CÁC MODEL NHẬN DỮ LIỆU NGAY TẠI ĐÂY
    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}