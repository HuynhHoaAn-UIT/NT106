using GlobalSuccess.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Co dinh cong 5116 cho Server
builder.WebHost.UseUrls("http://localhost:5116");

// 1. Doc cau hinh tu appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKeyStr = jwtSettings["SecretKey"] ?? "DayLaKhoaBiMatRatDaiVaBaoMatCuaNhomNT106KhongDuocTietLo123456!";
var secretKey = Encoding.UTF8.GetBytes(secretKeyStr);
var issuer = jwtSettings["Issuer"] ?? "GlobalSuccessBackend";
var audience = jwtSettings["Audience"] ?? "GlobalSuccessDesktopClient";

// 2. Cau hinh JWT Authentication
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
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Endpoint kiem tra nhanh tren trinh duyet Web
app.MapGet("/", () => ">>> GlobalSuccess API dang chay thanh cong tai cong 5116! <<<");

app.MapControllers();

Console.WriteLine("==================================================");
Console.WriteLine(">>> SERVER GLOBAL SUCCESS API DANG HOAT DONG! <<<");
Console.WriteLine(">>> Dia chi lang nghe: http://localhost:5116  <<<");
Console.WriteLine("==================================================");

app.Run();
