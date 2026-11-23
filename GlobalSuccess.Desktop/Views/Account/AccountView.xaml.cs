using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GlobalSuccess.Desktop.Services;

namespace GlobalSuccess.Desktop.Views.Account
{
    public partial class AccountView : UserControl
    {
        public AccountView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserProfileAsync();
        }

        // Tải thông tin hồ sơ người dùng từ REST API
        private async Task LoadUserProfileAsync()
        {
            try
            {
                var response = await ApiClient.Http.GetAsync("api/user/profile");
                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
                    if (profile != null && !string.IsNullOrWhiteSpace(profile.FullName))
                    {
                        txtProfileFullName.Text = profile.FullName;
                        txtSidebarFullName.Text = profile.FullName;

                        string initials = GetInitials(profile.FullName);
                        txtProfileAvatar.Text = initials;
                        txtSidebarAvatar.Text = initials;
                    }
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi mạng khi tải hồ sơ nền
            }
        }

        // Trích xuất 2 chữ cái đầu đại diện avatar (VD: Nguyễn Kim Loan -> KL)
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "U";

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpper() : parts[0].ToUpper();

            return $"{char.ToUpper(parts[^2][0])}{char.ToUpper(parts[^1][0])}";
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new Home.HomeView());
        }

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new AccountSettingsView());
        }
    }

    public class UserProfileResponse
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
