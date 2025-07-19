using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using InvoiceWebApp.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace InvoiceWebApp.Services
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IConfiguration _configuration;

        public EmailService(IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailSettings = emailSettings.Value;
            _configuration = configuration;
        }

        public bool SendVerificationEmail(string toEmail, string fullName, string verificationToken)
        {
            try
            {
                var baseUrl = _configuration["AppSettings:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    Console.WriteLine("Error: AppSettings:BaseUrl is not configured.");
                    return false;
                }
                var verificationUrl = $"{baseUrl}/Account/VerifyEmail?token={verificationToken}";

                var subject = $"Verify Your Email Address - {_configuration["AppSettings:ApplicationName"]}";
                var body = GetVerificationEmailTemplate(fullName, verificationUrl);

                return SendEmail(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending verification email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        public bool SendPasswordResetEmail(string toEmail, string fullName, string resetToken)
        {
            try
            {
                var baseUrl = _configuration["AppSettings:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    Console.WriteLine("Error: AppSettings:BaseUrl is not configured.");
                    return false;
                }
                var resetUrl = $"{baseUrl}/Account/ResetPassword?token={resetToken}";

                var subject = $"Password Reset Request - {_configuration["AppSettings:ApplicationName"]}";
                var body = GetPasswordResetEmailTemplate(fullName, resetUrl);

                return SendEmail(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending password reset email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        public bool SendWelcomeEmail(string toEmail, string fullName)
        {
            try
            {
                var subject = $"Welcome to {_configuration["AppSettings:ApplicationName"]}! 🎉";
                var body = GetWelcomeEmailTemplate(fullName);

                return SendEmail(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending welcome email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        public bool SendInvoiceCreatedNotification(string toEmail, string fullName, int invoiceId, decimal totalAmount)
        {
            try
            {
                var subject = $"Invoice #{invoiceId} Created Successfully - {_configuration["AppSettings:ApplicationName"]}";
                var body = GetInvoiceCreatedTemplate(fullName, invoiceId, totalAmount);

                return SendEmail(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending invoice created notification to {toEmail}: {ex.Message}");
                return false;
            }
        }

        private bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                smtpClient.EnableSsl = _emailSettings.EnableSsl;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                smtpClient.Timeout = 30000;

                smtpClient.Send(mailMessage);
                return true;
            }
            catch (SmtpException sx)
            {
                Console.WriteLine($"SMTP Error to {toEmail} [{subject}]: {sx.StatusCode} - {sx.Message} Inner: {sx.InnerException?.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Email Send Error to {toEmail} [{subject}]: {ex.Message} Inner: {ex.InnerException?.Message}");
                return false;
            }
        }



        private string GetVerificationEmailTemplate(string fullName, string verificationUrl)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Email Verification</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; border: 1px solid #dee2e6; }}
                        .button {{
                            display: inline-block;
                            padding: 12px 24px;
                            background-color: #28a745;
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 20px 0;
                            font-weight: bold;
                        }}
                        .footer {{
                            padding: 20px;
                            text-align: center;
                            font-size: 12px;
                            color: #666;
                            background-color: #e9ecef;
                            border-radius: 0 0 5px 5px;
                        }}
                        .warning {{ background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎉 Welcome to {_configuration["AppSettings:ApplicationName"]}!</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {fullName},</h2>
                            <p>Thank you for registering with {_configuration["AppSettings:ApplicationName"]}. To complete your registration and start managing your invoices, please verify your email address by clicking the button below:</p>

                            <div style='text-align: center;'>
                                <a href='{verificationUrl}' class='button'>✅ Verify Email Address</a>
                            </div>

                            <div class='warning'>
                                <strong>⚠️ Important:</strong> This verification link will expire for security reasons.
                            </div>

                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; background-color: #e9ecef; padding: 10px; border-radius: 3px;'>{verificationUrl}</p>

                            <p>If you didn't create an account with us, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 {_configuration["AppSettings:ApplicationName"]}. All rights reserved.</p>
                            <p>This is an automated email. Please do not reply to this message.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetWelcomeEmailTemplate(string fullName)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Welcome</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; border: 1px solid #dee2e6; }}
                        .feature {{ background-color: white; padding: 15px; margin: 10px 0; border-radius: 5px; border-left: 4px solid #28a745; }}
                        .footer {{
                            padding: 20px;
                            text-align: center;
                            font-size: 12px;
                            color: #666;
                            background-color: #e9ecef;
                            border-radius: 0 0 5px 5px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎊 Welcome Aboard!</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {fullName},</h2>
                            <p><strong>Congratulations!</strong> Your email has been successfully verified and your account is now active. Welcome to the {_configuration["AppSettings:ApplicationName"]} family! 🎉</p>

                            <h3>🚀 You're Ready to Get Started!</h3>
                            <p>Your account is now fully activated. Here are the amazing features waiting for you:</p>

                            <div class='feature'>
                                <h4>📄 Create Professional Invoices</h4>
                                <p>Design and generate professional invoices with our easy-to-use interface.</p>
                            </div>

                            <div class='feature'>
                                <h4>📈 Track Your Business</h4>
                                <p>Monitor your billing history and keep track of all your transactions.</p>
                            </div>

                            <div class='feature'>
                                <h4>🌍 Multi-Language Support</h4>
                                <p>Switch between English and Arabic to work in your preferred language.</p>
                            </div>

                            <div class='feature'>
                                <h4>🔐 Secure Data Management</h4>
                                <p>Your data is protected with industry-standard security measures.</p>
                            </div>

                            <p style='text-align: center; margin: 30px 0;'>
                                <strong>Ready to create your first invoice?</strong><br>
                                Log in to your account and start managing your business more efficiently!
                            </p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 {_configuration["AppSettings:ApplicationName"]}. All rights reserved.</p>
                            <p>Need help? Contact us at {_configuration["AppSettings:SupportEmail"]}</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetPasswordResetEmailTemplate(string fullName, string resetUrl)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Password Reset</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; border: 1px solid #dee2e6; }}
                        .button {{
                            display: inline-block;
                            padding: 12px 24px;
                            background-color: #dc3545;
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 20px 0;
                            font-weight: bold;
                        }}
                        .footer {{
                            padding: 20px;
                            text-align: center;
                            font-size: 12px;
                            color: #666;
                            background-color: #e9ecef;
                            border-radius: 0 0 5px 5px;
                        }}
                        .security-notice {{
                            background-color: #d1ecf1;
                            padding: 15px;
                            border-radius: 5px;
                            margin: 15px 0;
                            border-left: 4px solid #bee5eb;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔐 Password Reset Request</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {fullName},</h2>
                            <p>We received a request to reset your password for your {_configuration["AppSettings:ApplicationName"]} account.</p>

                            <p>Click the button below to reset your password:</p>
                            <div style='text-align: center;'>
                                <a href='{resetUrl}' class='button'>🔑 Reset Password</a>
                            </div>

                            <div class='security-notice'>
                                <strong>🛡️ Security Notice:</strong> This password reset link will expire in 1 hour for your security.
                            </div>

                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; background-color: #e9ecef; padding: 10px; border-radius: 3px;'>{resetUrl}</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 {_configuration["AppSettings:ApplicationName"]}. All rights reserved.</p>
                            <p>This is an automated security email. Please do not reply to this message.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetInvoiceCreatedTemplate(string fullName, int invoiceId, decimal totalAmount)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Invoice Created</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #17a2b8; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; border: 1px solid #dee2e6; }}
                        .invoice-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
                        .footer {{
                            padding: 20px;
                            text-align: center;
                            font-size: 12px;
                            color: #666;
                            background-color: #e9ecef;
                            border-radius: 0 0 5px 5px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>📄 Invoice Created Successfully!</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {fullName},</h2>
                            <p>Great news! Your invoice has been created successfully in {_configuration["AppSettings:ApplicationName"]}.</p>

                            <div class='invoice-info'>
                                <h3>📋 Invoice Details:</h3>
                                <table style='width: 100%; border-collapse: collapse;'>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'><strong>Invoice ID:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>#{invoiceId}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'><strong>Total Amount:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #dee2e6; color: #28a745; font-weight: bold;'>{totalAmount:F2}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px;'><strong>Created Date:</strong></td>
                                        <td style='padding: 8px;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                                    </tr>
                                </table>
                            </div>

                            <p>Thank you for using {_configuration["AppSettings:ApplicationName"]} to manage your business invoicing needs!</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 {_configuration["AppSettings:ApplicationName"]}. All rights reserved.</p>
                            <p>Happy invoicing! 🚀</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}