using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GlobalSuccess.Desktop.Services;

namespace GlobalSuccess.Desktop.Views.Authentication
{
    public partial class SignUpView : UserControl
    {
        private bool _isPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        public SignUpView()
        {
            InitializeComponent();

            // Lắng nghe thay đổi mật khẩu để cập nhật độ mạnh
            txtPassword.PasswordChanged += TxtPassword_PasswordChanged;

            // Đồng bộ 2 chiều khi người dùng gõ trong ô hiển thị mật khẩu rõ
            txtPasswordVisible.TextChanged += (s, e) =>
            {
                if (_isPasswordVisible && txtPassword.Password != txtPasswordVisible.Text)
                {
                    txtPassword.Password = txtPasswordVisible.Text;
                }
            };

            txtConfirmPasswordVisible.TextChanged += (s, e) =>
            {
                if (_isConfirmPasswordVisible && txtConfirmPassword.Password != txtConfirmPasswordVisible.Text)
                {
                    txtConfirmPassword.Password = txtConfirmPasswordVisible.Text;
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

        private void IconToggleConfirmPassword_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;

            if (_isConfirmPasswordVisible)
            {
                txtConfirmPasswordVisible.Text = txtConfirmPassword.Password;
                txtConfirmPassword.Visibility = Visibility.Collapsed;
                txtConfirmPasswordVisible.Visibility = Visibility.Visible;
                txtConfirmPasswordVisible.Focus();
                txtConfirmPasswordVisible.CaretIndex = txtConfirmPasswordVisible.Text.Length;

                iconToggleConfirmPassword.Foreground = new SolidColorBrush(Color.FromRgb(0x21, 0x1B, 0x70));
            }
            else
            {
                txtConfirmPassword.Password = txtConfirmPasswordVisible.Text;
                txtConfirmPasswordVisible.Visibility = Visibility.Collapsed;
                txtConfirmPassword.Visibility = Visibility.Visible;
                txtConfirmPassword.Focus();

                iconToggleConfirmPassword.Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
            }
        }
        // Calendar 
        private void BtnCalendar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DateTime.TryParseExact(txtDob.Text.Trim(), "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                calendarDob.DisplayDate = parsedDate;
                calendarDob.SelectedDate = parsedDate;
            }

            popupCalendar.IsOpen = true;
        }

        private void CalendarDob_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (calendarDob.SelectedDate.HasValue)
            {
                txtDob.Text = calendarDob.SelectedDate.Value.ToString("dd/MM/yyyy");
                popupCalendar.IsOpen = false;
            }
        }

        // ĐIỀU HƯỚNG
        private void BtnGoToSignIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new SignInView());
        }
        // REALTIME: Hiển thị độ mạnh mật khẩu khi đang gõ
        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string pw = txtPassword.Password;
            if (string.IsNullOrEmpty(pw))
            {
                panelPasswordStrength.Visibility = Visibility.Collapsed;
                return;
            }
            panelPasswordStrength.Visibility = Visibility.Visible;
            UpdateStrengthBar(GetPasswordScore(pw));
        }
        // TÍNH ĐIỂM ĐỘ MẠNH MẬT KHẨU (0 → 4)
        private int GetPasswordScore(string pw)
        {
            int score = 0;
            if (pw.Length >= 8) score++;
            if (Regex.IsMatch(pw, @"[A-Z]") && Regex.IsMatch(pw, @"[a-z]")) score++;
            if (Regex.IsMatch(pw, @"\d")) score++;
            if (Regex.IsMatch(pw, @"[!@#$%^&*()_+\-=\[\]]")) score++;
            return score;
        }
        // CẬP NHẬT THANH ĐỘ MẠNH
        private void UpdateStrengthBar(int score)
        {
            var gray   = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0));
            var red    = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35));
            var orange = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
            var green  = new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47));

            rect1.Fill = rect2.Fill = rect3.Fill = gray;

            switch (score)
            {
                case 1:
                    rect1.Fill = red;
                    txtStrengthLabel.Text = "Yếu";
                    txtStrengthLabel.Foreground = red;
                    break;
                case 2:
                    rect1.Fill = rect2.Fill = orange;
                    txtStrengthLabel.Text = "Trung bình";
                    txtStrengthLabel.Foreground = orange;
                    break;
                case >= 3:
                    rect1.Fill = rect2.Fill = rect3.Fill = green;
                    txtStrengthLabel.Text = "Mạnh";
                    txtStrengthLabel.Foreground = green;
                    break;
            }
        }
        // HELPERS: ẨN/HIỆN LỖI INLINE NGAY DƯỚI Ô NHẬP
        private void ShowError(TextBlock errorBlock, string msg)
        {
            errorBlock.Text = "⚠ " + msg;
            errorBlock.Visibility = Visibility.Visible;
        }

        private void HideAllErrors()
        {
            txtErrorFullName.Visibility  = Visibility.Collapsed;
            txtErrorEmail.Visibility     = Visibility.Collapsed;
            txtErrorPhone.Visibility     = Visibility.Collapsed;
            txtErrorPassword.Visibility  = Visibility.Collapsed;
        }
        // KIỂM TRA ĐỊNH DẠNG EMAIL
        private bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$");
        // VALIDATE TOÀN BỘ FORM
        private bool ValidateForm(string fullName, string email, string phone,
                                   string password, string confirmPassword)
        {
            HideAllErrors();
            bool isValid = true;

            // 1. Họ và tên
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowError(txtErrorFullName, "Vui lòng nhập Họ và tên!");
                isValid = false;
            }

            // 2. Email
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError(txtErrorEmail, "Vui lòng nhập Email!");
                isValid = false;
            }
            else if (!IsValidEmail(email))
            {
                ShowError(txtErrorEmail, "Email không đúng định dạng! (vd: abc@gmail.com)");
                isValid = false;
            }

            // 3. Số điện thoại 
            if (string.IsNullOrWhiteSpace(phone))
            {
                ShowError(txtErrorPhone, "Vui lòng nhập số điện thoại!");
                isValid = false;
            }
            else if (!phone.StartsWith("0"))
            {
                ShowError(txtErrorPhone, "Số điện thoại phải bắt đầu bằng số 0!");
                isValid = false;
            }
            else if (!phone.All(char.IsDigit))
            {
                ShowError(txtErrorPhone, "Số điện thoại chỉ được chứa các chữ số!");
                isValid = false;
            }
            else if (phone.Length != 10)
            {
                ShowError(txtErrorPhone, "Số điện thoại phải có đúng 10 chữ số!");
                isValid = false;
            }

            // 4. Mật khẩu
            if (string.IsNullOrEmpty(password))
            {
                ShowError(txtErrorPassword, "Vui lòng nhập mật khẩu!");
                isValid = false;
            }
            else if (password.Length < 8)
            {
                ShowError(txtErrorPassword, "Mật khẩu phải có ít nhất 8 ký tự!");
                isValid = false;
            }
            else if (GetPasswordScore(password) < 2)
            {
                ShowError(txtErrorPassword, "Mật khẩu quá yếu! Thêm chữ hoa, số hoặc ký tự đặc biệt.");
                isValid = false;
            }
            // 5. Confirm password
            else if (password != confirmPassword)
            {
                ShowError(txtErrorPassword, "Mật khẩu xác nhận không khớp!");
                isValid = false;
            }

            return isValid;
        }
        // TRẠNG THÁI LOADING
        private void SetLoading(bool isLoading)
        {
            btnSignUp.IsEnabled = !isLoading;
            btnSignUp.Content   = isLoading ? "Đang xử lý..." : "Đăng ký";
        }
        // SỰ KIỆN NHẤN NÚT ĐĂNG KÝ
        private async void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            string fullName        = txtFullName.Text.Trim();
            string email           = txtEmail.Text.Trim();
            string phone           = txtPhone.Text.Trim();
            string password        = _isPasswordVisible ? txtPasswordVisible.Text : txtPassword.Password;
            string confirmPassword = _isConfirmPasswordVisible ? txtConfirmPasswordVisible.Text : txtConfirmPassword.Password;

            if (!ValidateForm(fullName, email, phone, password, confirmPassword))
                return;

            SetLoading(true);

            try
            {
                var response = await ApiClient.Http.PostAsJsonAsync("api/auth/register",
                    new { FullName = fullName, Email = email, Password = password });

                if (response.IsSuccessStatusCode)
                {
                    MainWindow.Instance?.NavigateTo(new SignInView());
                    MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập.",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    ShowError(txtErrorEmail, errorMsg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối tới Server: {ex.Message}",
                    "Lỗi mạng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) { }
    }
}
