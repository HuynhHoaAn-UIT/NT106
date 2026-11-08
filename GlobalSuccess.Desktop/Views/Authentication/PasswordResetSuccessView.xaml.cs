using System.Windows;
using System.Windows.Controls;

namespace GlobalSuccess.Desktop.Views.Authentication
{
    public partial class PasswordResetSuccessView : UserControl
    {
        public PasswordResetSuccessView()
        {
            InitializeComponent();
        }

        // Bấm nút "Đăng nhập lại" -> Quay về trang SignInView để đăng nhập bằng mật khẩu mới
        private void BtnGoToSignIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new SignInView());
        }
    }
}