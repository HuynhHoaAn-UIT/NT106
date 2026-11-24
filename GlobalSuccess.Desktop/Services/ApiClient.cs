using System.Net.Http;
using System.Net.Http.Headers;

namespace GlobalSuccess.Desktop.Services
{
    public static class ApiClient
    {
        public static HttpClient Http { get; } = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5116/") // Địa chỉ chạy của GlobalSuccess.Api
        };

        public static string? SavedToken { get; private set; }
        public static string? CurrentAvatarUrl { get; set; }
        public static string? CurrentFullName { get; set; }

        // Gọi hàm này sau khi đăng nhập thành công
        public static void SetBearerToken(string token)
        {
            SavedToken = token;
            // GẮN TOKEN VÀO AUTHORIZATION HEADER
            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ClearToken()
        {
            SavedToken = null;
            CurrentAvatarUrl = null;
            CurrentFullName = null;
            Http.DefaultRequestHeaders.Authorization = null;
        }
    }
}