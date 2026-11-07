using System.Net.Http;
using System.Net.Http.Headers;

namespace GlobalSuccess.Desktop.Services
{
    public static class ApiClient
    {
        public static HttpClient Http { get; } = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/") // Địa chỉ chạy của GlobalSuccess.Api
        };

        public static string? SavedToken { get; private set; }

        // Gọi hàm này sau khi đăng nhập thành công
        public static void SetBearerToken(string token)
        {
            SavedToken = token;
            // GẮN TOKEN VÀO AUTHORIZATION HEADER (Đúng theo yêu cầu Task 26)
            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ClearToken()
        {
            SavedToken = null;
            Http.DefaultRequestHeaders.Authorization = null;
        }
    }
}