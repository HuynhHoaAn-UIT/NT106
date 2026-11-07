using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using GlobalSuccess.Desktop.Services;

namespace GlobalSuccess.Desktop.Views.Authentication
{
    public partial class SignUpView : UserControl
    {
        public SignUpView()
        {
            InitializeComponent();
        }
        private void BtnGoToSignIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new SignInView());
        }
        private async void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Họ tên, Email và Mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var registerData = new
                {
                    FullName = fullName,
                    Email = email,
                    Password = password
                };

                // Gửi request tới Server Web API
                var response = await ApiClient.Http.PostAsJsonAsync("api/auth/register", registerData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Đăng ký thành công! Hãy chuyển sang Đăng nhập.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtPassword.Clear();
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Đăng ký thất bại: {errorMsg}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối tới Server: {ex.Message}", "Lỗi mạng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}