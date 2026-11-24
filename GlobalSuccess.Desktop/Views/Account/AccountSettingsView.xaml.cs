using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GlobalSuccess.Desktop.Services;

namespace GlobalSuccess.Desktop.Views.Account
{
    public partial class AccountSettingsView : UserControl
    {
        private string? _currentAvatarUrl;

        public AccountSettingsView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserProfileAsync();
        }

        // Tải thông tin hồ sơ hiện tại lên form cài đặt
        private async Task LoadUserProfileAsync()
        {
            // Hiển thị ngay từ bộ nhớ đệm nếu đã có
            if (!string.IsNullOrWhiteSpace(ApiClient.CurrentFullName))
            {
                txtFullName.Text = ApiClient.CurrentFullName;
                txtCardFullName.Text = ApiClient.CurrentFullName;
                txtSidebarFullName.Text = ApiClient.CurrentFullName;
                string initials = GetInitials(ApiClient.CurrentFullName);
                txtCardAvatar.Text = initials;
                txtSidebarAvatar.Text = initials;
                txtTopbarAvatar.Text = initials;
            }
            if (!string.IsNullOrWhiteSpace(ApiClient.CurrentAvatarUrl))
            {
                _currentAvatarUrl = ApiClient.CurrentAvatarUrl;
                SetAvatarImage(imgCardAvatar, txtCardAvatar, _currentAvatarUrl);
                SetAvatarImage(imgSidebarAvatar, txtSidebarAvatar, _currentAvatarUrl);
                SetAvatarImage(imgTopbarAvatar, txtTopbarAvatar, _currentAvatarUrl);
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
                            txtFullName.Text = profile.FullName;
                            txtCardFullName.Text = profile.FullName;
                            txtSidebarFullName.Text = profile.FullName;

                            string initials = GetInitials(profile.FullName);
                            txtCardAvatar.Text = initials;
                            txtSidebarAvatar.Text = initials;
                            txtTopbarAvatar.Text = initials;
                        }

                        if (!string.IsNullOrWhiteSpace(profile.Email))
                        {
                            txtEmail.Text = profile.Email;
                            txtCardEmail.Text = profile.Email;
                        }

                        if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
                        {
                            ApiClient.CurrentAvatarUrl = profile.AvatarUrl;
                            _currentAvatarUrl = profile.AvatarUrl;
                            SetAvatarImage(imgCardAvatar, txtCardAvatar, _currentAvatarUrl);
                            SetAvatarImage(imgSidebarAvatar, txtSidebarAvatar, _currentAvatarUrl);
                            SetAvatarImage(imgTopbarAvatar, txtTopbarAvatar, _currentAvatarUrl);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi mạng khi tải dữ liệu nền
            }
        }

        // Mở hộp thoại chọn ảnh từ máy tính và tải lên server
        private async void BtnChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn ảnh đại diện",
                Filter = "Tệp ảnh (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|Tất cả tệp (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                    using var content = new MultipartFormDataContent();
                    using var byteContent = new ByteArrayContent(fileBytes);

                    string ext = Path.GetExtension(filePath).ToLowerInvariant();
                    string contentType = ext switch
                    {
                        ".png" => "image/png",
                        ".webp" => "image/webp",
                        _ => "image/jpeg"
                    };
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    content.Add(byteContent, "file", Path.GetFileName(filePath));

                    var response = await ApiClient.Http.PostAsync("api/user/upload-avatar", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<UploadAvatarResponse>();
                        if (result != null && !string.IsNullOrWhiteSpace(result.AvatarUrl))
                        {
                            _currentAvatarUrl = result.AvatarUrl;
                            ApiClient.CurrentAvatarUrl = _currentAvatarUrl;
                            SetAvatarImage(imgCardAvatar, txtCardAvatar, _currentAvatarUrl);
                            SetAvatarImage(imgSidebarAvatar, txtSidebarAvatar, _currentAvatarUrl);
                            SetAvatarImage(imgTopbarAvatar, txtTopbarAvatar, _currentAvatarUrl);
                            MessageBox.Show("Tải ảnh lên thành công! Nhấn 'Lưu' để hoàn tất cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Tải ảnh thất bại (HTTP {(int)response.StatusCode}): {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Cập nhật thông tin hồ sơ người dùng qua REST API: PUT /api/user/profile
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Vui lòng nhập họ và tên.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return;
            }

            try
            {
                var updateData = new
                {
                    FullName = fullName,
                    AvatarUrl = _currentAvatarUrl
                };

                var response = await ApiClient.Http.PutAsJsonAsync("api/user/profile", updateData);

                if (response.IsSuccessStatusCode)
                {
                    ApiClient.CurrentFullName = fullName;
                    ApiClient.CurrentAvatarUrl = _currentAvatarUrl;
                    MessageBox.Show("Cập nhật thông tin hồ sơ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    MainWindow.Instance?.NavigateTo(new AccountView());
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Cập nhật thất bại: {errorMsg}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối đến máy chủ: {ex.Message}", "Lỗi mạng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new AccountView());
        }

        private void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new AccountView());
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new Home.HomeView());
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
    }

    public class UploadAvatarResponse
    {
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
