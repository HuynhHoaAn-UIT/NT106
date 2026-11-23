using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
                    if (profile != null)
                    {
                        if (!string.IsNullOrWhiteSpace(profile.FullName))
                        {
                            txtProfileFullName.Text = profile.FullName;
                            txtSidebarFullName.Text = profile.FullName;

                            string initials = GetInitials(profile.FullName);
                            txtProfileAvatar.Text = initials;
                            txtSidebarAvatar.Text = initials;
                        }

                        if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
                        {
                            SetAvatarImage(imgProfileAvatar, txtProfileAvatar, profile.AvatarUrl);
                            SetAvatarImage(imgSidebarAvatar, txtSidebarAvatar, profile.AvatarUrl);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi mạng khi tải hồ sơ nền
            }
        }

        // Hiển thị ảnh đại diện hoặc fallback về chữ cái viết tắt
        private void SetAvatarImage(ImageBrush brush, TextBlock textBlock, string? url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(url, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    brush.ImageSource = bitmap;
                    textBlock.Visibility = Visibility.Collapsed;
                    return;
                }
                catch
                {
                    // Fallback ve chu cai viet tat neu khong tai duoc anh
                }
            }
            brush.ImageSource = null;
            textBlock.Visibility = Visibility.Visible;
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
