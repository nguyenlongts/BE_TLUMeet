using NotificationService.API.Events;
using NotificationService.API.Services;

namespace NotificationService.API.Consumers
{
    public class EmailVerificationConsumer : KafkaConsumerBase<EmailVerificationRequestedEvent>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        protected override string Topic => "email-verification-events";
        protected override string GroupId => "notification-service-email-verification";

        public EmailVerificationConsumer(IConfiguration configuration, ILogger<EmailVerificationConsumer> logger, IServiceProvider serviceProvider) : base(configuration, logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ProcessMessageAsync(EmailVerificationRequestedEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var fe = _configuration["FE:BaseUrl"] ?? throw new InvalidOperationException("FE:BaseUrl is not configured");

            var verifyLink = $"{fe.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(message.VerificationToken)}";

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>Xác thực tài khoản TLUMeet</h2>
                    <p>Xin chào <strong>{message.UserName}</strong>,</p>
                    <p>Cảm ơn bạn đã đăng ký TLUMeet. Vui lòng xác thực email để kích hoạt tài khoản:</p>
                    <p style='margin: 30px 0;'>
                        <a href='{verifyLink}'
                           style='background-color:#2563eb;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;'>
                            Xác thực email
                        </a>
                    </p>
                    <p style='color:#6b7280;'>Link có hiệu lực đến {message.ExpiresAt:HH:mm dd/MM/yyyy} (UTC).</p>
                    <p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                    <p style='color:#6b7280;font-size:12px;'>
                        Nếu nút không hoạt động, copy link sau vào trình duyệt:<br>{verifyLink}
                    </p>
                </div>";

            await emailService.SendEmailAsync(
                message.Email,
                "Xác thực tài khoản TLUMeet",
                emailBody,
                "EmailVerification");
        }
    }
}
