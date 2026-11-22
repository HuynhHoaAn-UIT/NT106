using Dapper;
using GlobalSuccess.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace GlobalSuccess.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly JwtTokenService _jwtService;
        private readonly EmailService _emailService;
        private readonly string _connectionString;
        // Danh sách lưu tạm các mã OTP trong RAM
        private static List<OtpRecord> _otpList = new List<OtpRecord>();

        public AuthController(IConfiguration config, JwtTokenService jwtService, EmailService emailService)
        {
            _config = config;
            _jwtService = jwtService;
            _emailService = emailService;
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

            return Ok("Đăng ký tài khoản thành công!");
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
        // 4. API GỬI MÃ OTP: POST /api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Vui lòng nhập Email.");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Kiểm tra email có trong bảng Users chưa
            string checkSql = "SELECT COUNT(1) FROM Users WHERE email = @Email";
            int exists = await connection.ExecuteScalarAsync<int>(checkSql, new { request.Email });
            if (exists == 0)
                return NotFound("Email này chưa được đăng ký trong hệ thống.");

            // Sinh mã OTP ngẫu nhiên 6 chữ số (từ 100000 đến 999999)
            string otp = Random.Shared.Next(100000, 999999).ToString();

            //Tìm xem email này đã có trong danh sách chưa
            var daCo = _otpList.FirstOrDefault(x => x.Email == request.Email);

            if (daCo != null)
            {
                // ĐÃ CÓ: Chỉ cần cập nhật đè mã OTP mới và giờ hết hạn mới
                daCo.Otp = otp;
                daCo.ExpireTime = DateTime.Now.AddMinutes(5);
            }
            else
            {
                // CHƯA CÓ: Tạo một phiếu mới và thêm vào danh sách
                OtpRecord phieuMoi = new OtpRecord();
                phieuMoi.Email = request.Email;
                phieuMoi.Otp = otp;
                phieuMoi.ExpireTime = DateTime.Now.AddMinutes(5);

                _otpList.Add(phieuMoi);
            }

            try
            {
                await _emailService.SendOtpAsync(request.Email.Trim(), otp);
            }
            catch (Exception)
            {
                return StatusCode(503, "Không gửi được OTP qua Azure Email.");
            }

            return Ok("Mã OTP đã được gửi qua email!");
        }

        // 5. API KIỂM TRA MÃ OTP: POST /api/auth/verify-otp
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
                return BadRequest("Email và Mã OTP không được để trống.");

            // Tìm xem email này có mã OTP nào trong danh sách không
            var record = _otpList.FirstOrDefault(x => x.Email == request.Email.Trim());

            if (record == null)
                return BadRequest("Chưa có yêu cầu mã OTP cho email này.");

            if (DateTime.Now > record.ExpireTime)
            {
                _otpList.Remove(record);
                return BadRequest("Mã OTP đã hết hạn (quá 5 phút).");
            }

            if (record.Otp != request.Otp.Trim())
                return BadRequest("Mã xác thực OTP không chính xác.");

            return Ok("Mã OTP chính xác!");
        }

        // 6. API ĐẶT LẠI MẬT KHẨU MỚI: POST /api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("Thông tin không hợp lệ.");

            if (request.NewPassword.Length < 8)
                return BadRequest("Mật khẩu mới phải có ít nhất 8 ký tự.");

            // Kiểm tra mã OTP một lần nữa trước khi cho đổi
            var record = _otpList.FirstOrDefault(x => x.Email == request.Email.Trim());
            if (record == null || record.Otp != request.Otp?.Trim())
                return BadRequest("Mã OTP không hợp lệ hoặc đã hết hạn.");

            // Băm mật khẩu mới bằng BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Cập nhật mật khẩu mới vào bảng Users trên Azure
            string updateSql = "UPDATE Users SET password_hash = @PasswordHash WHERE email = @Email";
            await connection.ExecuteAsync(updateSql, new { PasswordHash = passwordHash, Email = request.Email.Trim() });

            // Xóa mã OTP đi sau khi đổi thành công để không bị dùng lại
            _otpList.Remove(record);

            return Ok("Đặt lại mật khẩu thành công!" );
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

    // Đối tượng lưu thông tin 1 mã OTP
    public class OtpRecord
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public DateTime ExpireTime { get; set; }
    }
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
