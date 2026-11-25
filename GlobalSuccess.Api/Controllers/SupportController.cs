using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace GlobalSuccess.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Bắt buộc đăng nhập bằng JWT Token mới được gửi hoặc xem yêu cầu
    public class SupportController : ControllerBase
    {
        private readonly string _connectionString;

        public SupportController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:DefaultConnection trong appsettings.json");
        }

        // 1. API TIẾP NHẬN YÊU CẦU HỖ TRỢ / KHIẾU NẠI: POST /api/support/tickets
        [HttpPost("tickets")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
                return Unauthorized("Token không hợp lệ.");

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Tiêu đề yêu cầu không được để trống.");

            if (string.IsNullOrWhiteSpace(request.Category))
                return BadRequest("Vui lòng chọn loại yêu cầu hỗ trợ.");

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Nội dung chi tiết không được để trống.");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO SupportTickets (user_id, title, category, content, status, created_at)
                VALUES (@UserId, @Title, @Category, @Content, 'Pending', NOW());
                SELECT LAST_INSERT_ID();";

            var ticketId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                UserId = userId,
                Title = request.Title.Trim(),
                Category = request.Category.Trim(),
                Content = request.Content.Trim()
            });

            return Ok(new
            {
                ticketId = ticketId,
                message = "Gửi yêu cầu hỗ trợ thành công! Đội ngũ sẽ sớm phản hồi."
            });
        }

        // 2. API XEM DANH SÁCH YÊU CẦU CỦA NGƯỜI DÙNG: GET /api/support/my-tickets
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
                return Unauthorized("Token không hợp lệ.");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                SELECT 
                    ticket_id AS TicketId,
                    user_id AS UserId,
                    title AS Title,
                    category AS Category,
                    content AS Content,
                    status AS Status,
                    created_at AS CreatedAt
                FROM SupportTickets
                WHERE user_id = @UserId
                ORDER BY created_at DESC";

            var tickets = await connection.QueryAsync<SupportTicketResponse>(sql, new { UserId = userId });

            return Ok(tickets);
        }

        // 3. API XEM CHI TIẾT 1 YÊU CẦU: GET /api/support/tickets/{id}
        [HttpGet("tickets/{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
                return Unauthorized("Token không hợp lệ.");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                SELECT 
                    ticket_id AS TicketId,
                    user_id AS UserId,
                    title AS Title,
                    category AS Category,
                    content AS Content,
                    status AS Status,
                    created_at AS CreatedAt
                FROM SupportTickets
                WHERE ticket_id = @TicketId AND user_id = @UserId
                LIMIT 1";

            var ticket = await connection.QuerySingleOrDefaultAsync<SupportTicketResponse>(sql, new
            {
                TicketId = id,
                UserId = userId
            });

            if (ticket == null)
                return NotFound("Không tìm thấy yêu cầu hỗ trợ này.");

            return Ok(ticket);
        }
    }

    // DTO nhận dữ liệu gửi yêu cầu
    public class CreateTicketRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    // DTO trả về thông tin yêu cầu hỗ trợ
    public class SupportTicketResponse
    {
        public int TicketId { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
