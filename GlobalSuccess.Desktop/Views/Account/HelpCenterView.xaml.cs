using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GlobalSuccess.Desktop.Services;

namespace GlobalSuccess.Desktop.Views.Account
{
    public partial class HelpCenterView : UserControl
    {
        public HelpCenterView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserProfileAsync();
            await LoadMyTicketsAsync();
        }

        // Tải thông tin hồ sơ người dùng để hiển thị trên avatar và sidebar
        private async Task LoadUserProfileAsync()
        {
            // Hiển thị ngay từ bộ nhớ đệm nếu đã có
            if (!string.IsNullOrWhiteSpace(ApiClient.CurrentFullName))
            {
                txtSidebarFullName.Text = ApiClient.CurrentFullName;
                string initials = GetInitials(ApiClient.CurrentFullName);
                txtSidebarAvatar.Text = initials;
                txtTopbarAvatar.Text = initials;
            }
            if (!string.IsNullOrWhiteSpace(ApiClient.CurrentAvatarUrl))
            {
                SetAvatarImage(imgSidebarAvatar, txtSidebarAvatar, ApiClient.CurrentAvatarUrl);
                SetAvatarImage(imgTopbarAvatar, txtTopbarAvatar, ApiClient.CurrentAvatarUrl);
            }

            try
            {
                var response = await ApiClient.Http.GetAsync("api/user/profile");
                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
                    if (profile != null)
                    {
                        if (!string.IsNullOrWhiteSpace(profile.FullName))
                        {
                            ApiClient.CurrentFullName = profile.FullName;
                            txtSidebarFullName.Text = profile.FullName;

                            string initials = GetInitials(profile.FullName);
                            txtSidebarAvatar.Text = initials;
                            txtTopbarAvatar.Text = initials;
                        }

                        if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
                        {
                            ApiClient.CurrentAvatarUrl = profile.AvatarUrl;
                            SetAvatarImage(imgSidebarAvatar, txtSidebarAvatar, profile.AvatarUrl);
                            SetAvatarImage(imgTopbarAvatar, txtTopbarAvatar, profile.AvatarUrl);
                        }
                    }
                }
            }
            catch
            {
                // Giữ nguyên giao diện mặc định nếu không tải được profile
            }
        }

        // Tải danh sách yêu cầu hỗ trợ cá nhân của người dùng
        private async Task LoadMyTicketsAsync()
        {
            try
            {
                var response = await ApiClient.Http.GetAsync("api/support/my-tickets");
                if (response.IsSuccessStatusCode)
                {
                    var tickets = await response.Content.ReadFromJsonAsync<List<SupportTicketResponse>>();
                    if (tickets != null && tickets.Count > 0)
                    {
                        var viewModels = tickets.Select(t => new SupportTicketItemViewModel
                        {
                            TicketId = t.TicketId,
                            Title = t.Title,
                            Category = t.Category,
                            Content = t.Content,
                            Status = t.Status,
                            CreatedAt = t.CreatedAt
                        }).ToList();

                        icMyTickets.ItemsSource = viewModels;
                        lblNoTickets.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        icMyTickets.ItemsSource = null;
                        lblNoTickets.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    lblNoTickets.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                lblNoTickets.Visibility = Visibility.Visible;
            }
        }

        // Làm mới danh sách yêu cầu hỗ trợ
        private async void BtnRefreshTickets_Click(object sender, RoutedEventArgs e)
        {
            await LoadMyTicketsAsync();
        }

        // Mở dialog khi bấm "Báo cáo sự cố"
        private void BtnReportIssue_Click(object sender, RoutedEventArgs e)
        {
            cboCategory.SelectedIndex = 0; // Báo lỗi ứng dụng
            txtTicketTitle.Text = string.Empty;
            txtTicketContent.Text = string.Empty;
            pnlTicketDialog.Visibility = Visibility.Visible;
        }

        // Mở dialog khi bấm "Gửi góp ý ngay"
        private void BtnFeedback_Click(object sender, RoutedEventArgs e)
        {
            cboCategory.SelectedIndex = 1; // Góp ý trải nghiệm
            txtTicketTitle.Text = string.Empty;
            txtTicketContent.Text = string.Empty;
            pnlTicketDialog.Visibility = Visibility.Visible;
        }

        // Đóng dialog gửi yêu cầu
        private void BtnCloseDialog_Click(object sender, RoutedEventArgs e)
        {
            pnlTicketDialog.Visibility = Visibility.Collapsed;
        }

        // Gửi yêu cầu hỗ trợ / khiếu nại lên server
        private async void BtnSubmitTicket_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTicketTitle.Text.Trim();
            string content = txtTicketContent.Text.Trim();
            string category = (cboCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Báo lỗi ứng dụng";

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Vui lòng nhập tiêu đề yêu cầu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTicketTitle.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Vui lòng nhập nội dung chi tiết.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTicketContent.Focus();
                return;
            }

            var request = new CreateTicketRequest
            {
                Title = title,
                Category = category,
                Content = content
            };

            try
            {
                var response = await ApiClient.Http.PostAsJsonAsync("api/support/tickets", request);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Yêu cầu của bạn đã được gửi thành công! Đội ngũ hỗ trợ sẽ phản hồi trong thời gian sớm nhất.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    pnlTicketDialog.Visibility = Visibility.Collapsed;
                    await LoadMyTicketsAsync();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Không thể gửi yêu cầu: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối khi gửi yêu cầu: {ex.Message}", "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Đặt ảnh đại diện hoặc hiển thị chữ cái viết tắt
        private void SetAvatarImage(ImageBrush brush, TextBlock textBlock, string relativeOrAbsoluteUrl)
        {
            if (!string.IsNullOrWhiteSpace(relativeOrAbsoluteUrl))
            {
                try
                {
                    string fullUrl = relativeOrAbsoluteUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? relativeOrAbsoluteUrl
                        : new Uri(ApiClient.Http.BaseAddress!, relativeOrAbsoluteUrl).ToString();

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullUrl, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    brush.ImageSource = bitmap;
                    textBlock.Visibility = Visibility.Collapsed;
                    return;
                }
                catch
                {
                    // Fallback về chữ cái viết tắt nếu không tải được ảnh
                }
            }
            brush.ImageSource = null;
            textBlock.Visibility = Visibility.Visible;
        }

        // Trích xuất 2 chữ cái đầu viết tắt cho avatar
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "U";

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpper() : parts[0].ToUpper();

            return $"{char.ToUpper(parts[^2][0])}{char.ToUpper(parts[^1][0])}";
        }

        // Điều hướng thanh bên
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new Home.HomeView());
        }

        private void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new AccountView());
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new AccountSettingsView());
        }
    }

    // DTO nhận dữ liệu gửi yêu cầu
    public class CreateTicketRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    // DTO trả về thông tin yêu cầu hỗ trợ từ API
    public class SupportTicketResponse
    {
        public int TicketId { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }

    // ViewModel hiển thị danh sách yêu cầu hỗ trợ với badge trạng thái
    public class SupportTicketItemViewModel
    {
        public int TicketId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }

        public string FormattedDate => CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        public string StatusText => Status == "Resolved" ? "Đã giải quyết" : "Đang xử lý";

        public Brush StatusBgBrush => Status == "Resolved"
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));

        public Brush StatusFgBrush => Status == "Resolved"
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706"));
    }
}
