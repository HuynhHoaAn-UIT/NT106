using GlobalSuccess.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Đọc cấu hình từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

// 2. Cấu hình cơ chế soát vé JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey), // Soát lại chữ ký bằng SecretKey

        ValidateLifetime = true, // Kiểm tra xem token còn hạn không
        ClockSkew = TimeSpan.Zero // Không cho phép độ trễ giờ
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

// 3. KÍCH HOẠT THEO ĐÚNG THỨ TỰ (Cực kỳ quan trọng)
app.UseAuthentication(); // 1. Xác thực (Bạn là ai?)
app.UseAuthorization();  // 2. Phân quyền (Bạn được làm gì?)

app.MapControllers();
app.Run();