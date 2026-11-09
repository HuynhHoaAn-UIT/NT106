using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GlobalSuccess.Desktop.Services;

namespace GlobalSuccess.Desktop.Views.Authentication
{
    public partial class ResetPasswordView : UserControl
    {
        private string _email = string.Empty;
        private string _otp = string.Empty;

        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        public ResetPasswordView()
        {
            InitializeComponent();

            // Đồng bộ 2 chiều khi người dùng gõ vào ô hiển thị mật khẩu rõ
            NewPasswordVisibleBox.TextChanged += (s, e) =>
            {
                if (_isNewPasswordVisible && NewPasswordBox.Password != NewPasswordVisibleBox.Text)
                {
                    NewPasswordBox.Password = NewPasswordVisibleBox.Text;
                }
            };

            ConfirmPasswordVisibleBox.TextChanged += (s, e) =>
            {
                if (_isConfirmPasswordVisible && ConfirmPasswordBox.Password != ConfirmPasswordVisibleBox.Text)
                {
                    ConfirmPasswordBox.Password = ConfirmPasswordVisibleBox.Text;
                }
            };
        }

        public ResetPasswordView(string email, string otp) : this()
        {
            _email = email;
            _otp = otp;
        }
        // BẬT / TẮT HIỂN THỊ MẬT KHẨU MỚI
        private void IconToggleNewPassword_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isNewPasswordVisible = !_isNewPasswordVisible;

            if (_isNewPasswordVisible)
            {
                NewPasswordVisibleBox.Text = NewPasswordBox.Password;
                NewPasswordBox.Visibility = Visibility.Collapsed;
                NewPasswordVisibleBox.Visibility = Visibility.Visible;
                NewPasswordVisibleBox.Focus();
                NewPasswordVisibleBox.CaretIndex = NewPasswordVisibleBox.Text.Length;

                iconToggleNewPassword.Foreground = new SolidColorBrush(Color.FromRgb(0x21, 0x1B, 0x70));
            }
            else
            {
                NewPasswordBox.Password = NewPasswordVisibleBox.Text;
                NewPasswordVisibleBox.Visibility = Visibility.Collapsed;
                NewPasswordBox.Visibility = Visibility.Visible;
                NewPasswordBox.Focus();

                iconToggleNewPassword.Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
            }
        }
        // BẬT / TẮT HIỂN THỊ XÁC NHẬN MẬT KHẨU
        private void IconToggleConfirmPassword_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;

            if (_isConfirmPasswordVisible)
            {
                ConfirmPasswordVisibleBox.Text = ConfirmPasswordBox.Password;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordVisibleBox.Visibility = Visibility.Visible;
                ConfirmPasswordVisibleBox.Focus();
                ConfirmPasswordVisibleBox.CaretIndex = ConfirmPasswordVisibleBox.Text.Length;

                iconToggleConfirmPassword.Foreground = new SolidColorBrush(Color.FromRgb(0x21, 0x1B, 0x70));
            }
            else
            {
                ConfirmPasswordBox.Password = ConfirmPasswordVisibleBox.Text;
                ConfirmPasswordVisibleBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = Visibility.Visible;
                ConfirmPasswordBox.Focus();

                iconToggleConfirmPassword.Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
            }
        }
        // BẤM NÚT "ĐẶT LẠI MẬT KHẨU"
        private async void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = _isNewPasswordVisible ? NewPasswordVisibleBox.Text : NewPasswordBox.Password;
            string confirmPassword = _isConfirmPasswordVisible ? ConfirmPasswordVisibleBox.Text : ConfirmPasswordBox.Password;

            // Kiểm tra mật khẩu
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ mật khẩu mới và xác nhận mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 8)
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất 8 ký tự!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Gửi mật khẩu mới lên Server để lưu vào MySQL
                var response = await ApiClient.Http.PostAsJsonAsync("api/auth/reset-password", new
                {
                    Email = _email,
                    Otp = _otp,
                    NewPassword = newPassword
                });

                if (response.IsSuccessStatusCode)
                {
                    MainWindow.Instance?.NavigateTo(new PasswordResetSuccessView());
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(errorMsg, "Lỗi đặt lại mật khẩu", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối Server: {ex.Message}", "Lỗi mạng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // BẤM "QUAY LẠI ĐĂNG NHẬP"
        private void BtnBackToSignIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new SignInView());
        }
    }
}
