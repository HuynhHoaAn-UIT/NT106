using System.Net.Http.Json;           // <-- Khắc phục lỗi CS1061 (PostAsJsonAsync)
using System.Windows;
using System.Windows.Controls;
using GlobalSuccess.Desktop.Services; // <-- Để gọi ApiClient

namespace GlobalSuccess.Desktop.Views.Authentication
{
    public partial class SignInView : UserControl
    {
        public SignInView()
        {
            InitializeComponent();
        }
        private void BtnGoToSignUp_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new SignUpView());
        }
        // Sự kiện khi bấm nút Đăng nhập
        private async void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Email và Mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var loginData = new { Email = email, Password = password };

                // Gửi request tới Server Web API
                var response = await ApiClient.Http.PostAsJsonAsync("api/auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    // Đọc Token trả về từ Server
                    var result = await response.Content.ReadFromJsonAsync<LoginResult>();

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        // 1. Lưu Token và tự động gắn vào Header (Task 26)
                        ApiClient.SetBearerToken(result.Token);

                        MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        // TODO: Chuyển sang màn hình HomeView của bạn
                    }
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Đăng nhập thất bại: {errorMsg}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối đến Server API: {ex.Message}", "Lỗi mạng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển sang màn hình Quên mật khẩu
            MainWindow.Instance?.NavigateTo(new ForgotPasswordView());
        }
    }

    // Khai báo model nhận dữ liệu Token từ API trả về
    public class LoginResult
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}