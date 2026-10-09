using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ScholarshipGateway
{
    public partial class OTPForm : Form
    {
        public OTPForm()
        {
            InitializeComponent();
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            string otp = txtOTP.Text;

            if (otp == "")
            {
                MessageBox.Show("Vui lòng nhập mã OTP!");
                return;
            }

            MessageBox.Show("Xác thực thành công!");

            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
            this.Hide();
        }

        private void btnResend_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng gửi lại OTP sẽ được bổ sung sau!");
        }
    }
}
