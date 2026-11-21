using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using GlobalSuccess.Desktop.Services;

namespace GlobalSuccess.Desktop.Views.Authentication
{
    public partial class VerifyOtpView : UserControl
    {
        // Biến lưu email được truyền sang từ màn hình trước
        private string _email = string.Empty;

        public VerifyOtpView()
        {
            InitializeComponent();
        }

        // HÀM TẠO NHẬN THAM SỐ EMAIL (Sẽ xóa lỗi đỏ ở ForgotPasswordView)
        public VerifyOtpView(string email) : this()
        {
            _email = email;
            txtDisplayEmail.Text = email; // Hiển thị email lên giao diện
        }

        // 1. Bấm nút "Xác thực mã"
        private async void BtnVerifyOtp_Click(object sender, RoutedEventArgs e)
        {
            // Lấy mã OTP và xóa khoảng trắng thừa nếu có
            string otp = OtpTextBox.Text.Trim().Replace(" ", "");

            if (string.IsNullOrEmpty(otp))
            {
                MessageBox.Show("Vui lòng nhập mã OTP gồm 6 chữ số!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Gửi mã OTP lên Server kiểm tra
                var response = await ApiClient.Http.PostAsJsonAsync("api/auth/verify-otp", new
                {
                    Email = _email,
                    Otp = otp
                });

                if (response.IsSuccessStatusCode)
                {
                    // Mã đúng! Chuyển sang màn hình Đặt lại mật khẩu mới
                    MainWindow.Instance?.NavigateTo(new ResetPasswordView(_email, otp));
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(errorMsg, "Xác thực thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối Server: {ex.Message}", "Lỗi mạng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 2. Bấm nút "Gửi lại mã"
        private async void BtnResendOtp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = await ApiClient.Http.PostAsJsonAsync("api/auth/forgot-password", new { Email = _email });
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Đã gửi mã OTP mới!\n(Kiểm tra màn hình Console Server để lấy mã)", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi lại mã: {ex.Message}", "Lỗi mạng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 3. Bấm "Đổi email" -> Quay về ForgotPasswordView
        private void BtnChangeEmail_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new ForgotPasswordView());
        }

        // 4. Bấm "Quay lại đăng nhập" -> Về SignInView
        private void BtnBackToSignIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new SignInView());
        }
    }
}