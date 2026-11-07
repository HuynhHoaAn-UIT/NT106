using System.Windows;
using System.Windows.Controls;
using GlobalSuccess.Desktop.Views.Authentication;

namespace GlobalSuccess.Desktop
{
    public partial class MainWindow : Window
    {
        // Cho phép các màn hình con gọi chuyển trang dễ dàng
        public static MainWindow? Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            // Mặc định khi mở ứng dụng sẽ hiện màn hình signin
            NavigateTo(new SignInView());
        }

        public void NavigateTo(UserControl view)
        {
            MainContent.Content = view;
        }
    }
}