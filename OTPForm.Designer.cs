namespace ScholarshipGateway
{
    partial class OTPForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblOTP = new Label();
            label2 = new Label();
            txtOTP = new MaskedTextBox();
            btnVerify = new Button();
            btnResend = new Button();
            btnBack = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // lblOTP
            // 
            lblOTP.AutoSize = true;
            lblOTP.Location = new Point(208, 158);
            lblOTP.Name = "lblOTP";
            lblOTP.Size = new Size(60, 20);
            lblOTP.TabIndex = 0;
            lblOTP.Text = "Mã OTP";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(258, 373);
            label2.Name = "label2";
            label2.Size = new Size(149, 20);
            label2.TabIndex = 1;
            label2.Text = "Chưa nhận được mã?";
            // 
            // txtOTP
            // 
            txtOTP.Location = new Point(208, 194);
            txtOTP.Name = "txtOTP";
            txtOTP.Size = new Size(334, 27);
            txtOTP.TabIndex = 2;
            // 
            // btnVerify
            // 
            btnVerify.Location = new Point(208, 247);
            btnVerify.Name = "btnVerify";
            btnVerify.Size = new Size(334, 29);
            btnVerify.TabIndex = 3;
            btnVerify.Text = "Xác thực";
            btnVerify.UseVisualStyleBackColor = true;
            btnVerify.Click += btnVerify_Click;
            // 
            // btnResend
            // 
            btnResend.Location = new Point(439, 369);
            btnResend.Name = "btnResend";
            btnResend.Size = new Size(94, 29);
            btnResend.TabIndex = 4;
            btnResend.Text = "Gửi lại mã";
            btnResend.UseVisualStyleBackColor = true;
            btnResend.Click += btnResend_Click;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(208, 300);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(334, 29);
            btnBack.TabIndex = 5;
            btnBack.Text = "Quay lại";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(225, 75);
            label1.Margin = new Padding(3);
            label1.Name = "label1";
            label1.Size = new Size(326, 58);
            label1.TabIndex = 6;
            label1.Text = "XÁC THỰC OTP";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // OTPForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label1);
            Controls.Add(btnBack);
            Controls.Add(btnResend);
            Controls.Add(btnVerify);
            Controls.Add(txtOTP);
            Controls.Add(label2);
            Controls.Add(lblOTP);
            Name = "OTPForm";
            Text = "Form3";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblOTP;
        private Label label2;
        private MaskedTextBox txtOTP;
        private Button btnVerify;
        private Button btnResend;
        private Button btnBack;
        private Label label1;
    }
}