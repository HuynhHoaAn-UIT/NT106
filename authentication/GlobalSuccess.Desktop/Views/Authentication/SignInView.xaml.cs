using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GlobalSuccess.Desktop.Services;

namespace GlobalSuccess.Desktop.Views.Authentication
{
    public partial class SignInView : UserControl
    {
        private bool _isPasswordVisible = false;

        public SignInView()
        {
            InitializeComponent();

            // Đồng bộ 2 chiều khi người dùng gõ phím ở ô hiện mật khẩu
            txtPasswordVisible.TextChanged += (s, e) =>
            {
                if (_isPasswordVisible && txtPassword.Password != txtPasswordVisible.Text)
                {
                    txtPassword.Password = txtPasswordVisible.Text;
                }
            };
        }
        // BẬT / TẮT HIỂN THỊ MẬT KHẨU
        private void IconTogglePassword_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPasswordVisible.Focus();
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;

                iconTogglePassword.Foreground = new SolidColorBrush(Color.FromRgb(0x21, 0x1B, 0x70));
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                txtPassword.Focus();

                iconTogglePassword.Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
            }
        }

        private void BtnGoToSignUp_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new SignUpView());
        }
        // BẤM NÚT ĐĂNG NHẬP
        private async void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = _isPasswordVisible ? txtPasswordVisible.Text : txtPassword.Password;

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
                        // Lưu Token và tự động gắn vào Header
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
