using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using GlobalSuccess.Desktop.Services;

namespace GlobalSuccess.Desktop.Views.Authentication
{
    public partial class ForgotPasswordView : UserControl
    {
        public ForgotPasswordView()
        {
            InitializeComponent();
        }

        // 1. Bấm nút "Quay lại đăng nhập"
        private void BtnBackToSignIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new SignInView());
        }

        // 2. Bấm nút "Gửi mã xác thực"
        private async void BtnSendOtp_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nhập email của bạn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Gửi yêu cầu lên Server để sinh mã OTP
                var response = await ApiClient.Http.PostAsJsonAsync("api/auth/forgot-password", new { Email = email });

                if (response.IsSuccessStatusCode)
                {
                    // Chuyển sang màn hình xác thực OTP và gửi kèm email vừa nhập
                    MainWindow.Instance?.NavigateTo(new VerifyOtpView(email));

                    MessageBox.Show("Mã OTP đã được gửi!\n(Bạn hãy nhìn cửa sổ đen Terminal của Server API để lấy mã)",
                                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Lỗi: {errorMsg}", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối Server: {ex.Message}", "Lỗi mạng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}