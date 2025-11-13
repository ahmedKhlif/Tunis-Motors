using System.Net;
using System.Net.Mail;

namespace TP2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");

                using var client = new SmtpClient(smtpSettings["SmtpServer"], int.Parse(smtpSettings["SmtpPort"]!))
                {
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]!)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["SenderEmail"]!, smtpSettings["SenderName"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Log the error - in production you'd want proper logging
                Console.WriteLine($"Email sending failed: {ex.Message}");
                // For now, we'll just continue without throwing
            }
        }

        public async Task SendOrderConfirmationAsync(string to, string customerName, int orderId, decimal totalAmount)
        {
            var subject = $"Order Confirmation - Order #{orderId}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunis Motors Logo' style='max-width: 200px; height: auto;' />
                    </div>
                    <h2 style='color: #28a745;'>Order Confirmation</h2>
                    <p>Dear {customerName},</p>
                    <p>Thank you for your order! Your order has been successfully placed.</p>

                    <div style='background: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <h4>Order Details:</h4>
                        <p><strong>Order ID:</strong> #{orderId}</p>
                        <p><strong>Total Amount:</strong> {totalAmount:N0} TND</p>
                        <p><strong>Status:</strong> Pending</p>
                    </div>

                    <p>You will receive updates on your order status via email.</p>
                    <p>If you have any questions, please contact our support team.</p>

                    <p>Best regards,<br>Tunis Motors Team</p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendOrderStatusUpdateAsync(string to, string customerName, int orderId, string status)
        {
            var subject = $"Order Status Update - Order #{orderId}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunis Motors Logo' style='max-width: 200px; height: auto;' />
                    </div>
                    <h2 style='color: #007bff;'>Order Status Update</h2>
                    <p>Dear {customerName},</p>
                    <p>Your order status has been updated.</p>

                    <div style='background: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <h4>Order Details:</h4>
                        <p><strong>Order ID:</strong> #{orderId}</p>
                        <p><strong>New Status:</strong> {status}</p>
                        <p><strong>Updated At:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                    </div>

                    <p>You can track your order status in your account dashboard.</p>

                    <p>Best regards,<br>Tunis Motors Team</p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetAsync(string to, string userName, string resetLink)
        {
            var subject = "Password Reset - Tunis Motors";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunis Motors Logo' style='max-width: 200px; height: auto;' />
                    </div>
                    <h2 style='color: #dc3545;'>Password Reset Request</h2>
                    <p>Dear {userName},</p>
                    <p>You have requested to reset your password for your Tunis Motors account.</p>
                    <p>Please click the button below to reset your password:</p>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                            Reset Password
                        </a>
                    </div>

                    <div style='background: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <p><strong>Important:</strong> This link will expire in 24 hours for security reasons.</p>
                        <p>If you didn't request a password reset, please ignore this email.</p>
                        <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #007bff;'>{resetLink}</p>
                    </div>

                    <p>For security reasons, we recommend choosing a strong password that includes a combination of letters, numbers, and special characters.</p>

                    <p>Best regards,<br>Tunis Motors Team</p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendCarSoldNotificationAsync(string to, string sellerName, string carName, decimal price)
        {
            var subject = $"Car Sold - {carName}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunis Motors Logo' style='max-width: 200px; height: auto;' />
                    </div>
                    <h2 style='color: #28a745;'>Congratulations! Your Car Has Been Sold</h2>
                    <p>Dear {sellerName},</p>
                    <p>Great news! Your car has been sold on Tunis Motors.</p>

                    <div style='background: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <h4>Sale Details:</h4>
                        <p><strong>Car:</strong> {carName}</p>
                        <p><strong>Sale Price:</strong> {price:N0} TND</p>
                        <p><strong>Sale Date:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                    </div>

                    <p>The buyer will contact you soon to arrange payment and delivery.</p>
                    <p>Please check your messages in the platform for buyer details.</p>

                    <p>Best regards,<br>Tunis Motors Team</p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendApprovalRequiredNotificationAsync(string to, string adminName, string carName, string sellerName)
        {
            var subject = $"New Car Listing Requires Approval - {carName}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunis Motors Logo' style='max-width: 200px; height: auto;' />
                    </div>
                    <h2 style='color: #ffc107;'>New Listing Requires Approval</h2>
                    <p>Dear {adminName},</p>
                    <p>A new car listing has been submitted and requires your approval.</p>

                    <div style='background: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <h4>Listing Details:</h4>
                        <p><strong>Car:</strong> {carName}</p>
                        <p><strong>Seller:</strong> {sellerName}</p>
                        <p><strong>Submitted:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                    </div>

                    <p>Please review and approve or reject this listing in the admin dashboard.</p>

                    <p>Best regards,<br>Tunis Motors System</p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmailConfirmationAsync(string to, string userName, string confirmationLink)
        {
            var subject = "Confirm Your Email - Tunis Motors";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunis Motors Logo' style='max-width: 200px; height: auto;' />
                    </div>
                    <h2 style='color: #28a745;'>Welcome to Tunis Motors!</h2>
                    <p>Dear {userName},</p>
                    <p>Thank you for registering with Tunis Motors. To complete your registration and activate your account, please confirm your email address.</p>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationLink}' style='background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                            Confirm Email Address
                        </a>
                    </div>

                    <div style='background: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <p><strong>Important:</strong> This link will expire in 24 hours for security reasons.</p>
                        <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #007bff;'>{confirmationLink}</p>
                    </div>

                    <p>If you didn't create an account with Tunis Motors, please ignore this email.</p>

                    <p>Best regards,<br>Tunis Motors Team</p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }
    }
}