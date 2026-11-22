
using GlobalSuccess.Desktop.Views.Account;
using System.Windows;
using System.Windows.Controls;

namespace GlobalSuccess.Desktop.Views.Home
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }
        private void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new AccountView());
        }
    }
}
