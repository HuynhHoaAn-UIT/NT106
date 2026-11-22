
using System.Windows;
using System.Windows.Controls;

namespace GlobalSuccess.Desktop.Views.Account
{
    public partial class AccountView : UserControl
    {
        public AccountView()
        {
            InitializeComponent();
        }
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new Home.HomeView());
        }

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new AccountSettingsView());
        }
    }
}
