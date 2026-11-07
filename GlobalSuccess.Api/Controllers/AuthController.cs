using GlobalSuccess.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlobalSuccess.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _jwtService;
        public AuthController(JwtTokenService jwtService) => _jwtService = jwtService;

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // 1. Kiểm tra database MySQL trên Azure xem email/password đúng không
            // (Ví dụ tạm kiểm tra thành công tìm ra user_id = 123)
            bool isSuccess = true;

            if (!isSuccess)
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng!" });

            // 2. Sinh chuỗi JWT Token
            string token = _jwtService.CreateToken(123, request.Email, "Nguyen Van A");

            // 3. Trả về cho Desktop Client
            return Ok(new { Token = token, Email = request.Email });
        }

        // ĐÂY LÀ VÍ DỤ API CẦN BẢO VỆ:
        [Authorize] // <--- Dòng này yêu cầu Client phải gửi Bearer Token thì mới cho truy cập
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            // Lấy thông tin user trực tiếp từ Token đã xác thực
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            return Ok(new { message = $"Xin chào {userEmail}, bạn đã xác thực JWT thành công!" });
        }
    }

    public record LoginRequest(string Email, string Password);
}