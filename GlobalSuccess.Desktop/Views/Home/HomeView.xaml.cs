using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GlobalSuccess.Desktop.Services;
using GlobalSuccess.Desktop.Views.Account;

namespace GlobalSuccess.Desktop.Views.Home
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserProfileAsync();
        }

        // Tải thông tin hồ sơ người dùng để hiển thị trên avatar và sidebar
        private async Task LoadUserProfileAsync()
        {
            // Hiển thị ngay từ bộ nhớ đệm nếu đã có
            if (!string.IsNullOrWhiteSpace(ApiClient.CurrentFullName))
            {
                txtSidebarFullName.Text = ApiClient.CurrentFullName;
                string initials = GetInitials(ApiClient.CurrentFullName);
                txtSidebarAvatar.Text = initials;
                txtTopbarAvatar.Text = initials;
            }
            if (!string.IsNullOrWhiteSpace(ApiClient.CurrentAvatarUrl))
            {
                SetAvatarImage(imgSidebarAvatar, txtSidebarAvatar, ApiClient.CurrentAvatarUrl);
                SetAvatarImage(imgTopbarAvatar, txtTopbarAvatar, ApiClient.CurrentAvatarUrl);
            }

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
                            ApiClient.CurrentFullName = profile.FullName;
                            txtSidebarFullName.Text = profile.FullName;
                            string initials = GetInitials(profile.FullName);
                            txtSidebarAvatar.Text = initials;
                            txtTopbarAvatar.Text = initials;
                        }

                        if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
                        {
                            ApiClient.CurrentAvatarUrl = profile.AvatarUrl;
                            SetAvatarImage(imgSidebarAvatar, txtSidebarAvatar, profile.AvatarUrl);
                            SetAvatarImage(imgTopbarAvatar, txtTopbarAvatar, profile.AvatarUrl);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi mạng khi tải dữ liệu nền
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
                    // Fallback ve chu cai viet tat neu khong load duoc anh
                }
            }
            brush.ImageSource = null;
            textBlock.Visibility = Visibility.Visible;
        }

        // Trích xuất 2 chữ cái đầu viết tắt cho avatar
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "U";

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpper() : parts[0].ToUpper();

            return $"{char.ToUpper(parts[^2][0])}{char.ToUpper(parts[^1][0])}";
        }

        private void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new AccountView());
        }
    }
}
