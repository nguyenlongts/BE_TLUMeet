using NotificationService.API.Events;
using NotificationService.API.Services;

namespace NotificationService.API.Consumers
{
    // Gửi email cảnh báo bảo mật khi mật khẩu tài khoản bị thay đổi.
    public class PasswordChangedConsumer : KafkaConsumerBase<PasswordChangedEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        protected override string Topic => "password-changed-events";
        protected override string GroupId => "notification-service-password-changed";

        public PasswordChangedConsumer(IConfiguration configuration, ILogger<PasswordChangedConsumer> logger, IServiceProvider serviceProvider) : base(configuration, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessMessageAsync(PasswordChangedEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var changedAt = message.ChangedAt.ToString("HH:mm dd/MM/yyyy") + " (UTC)";
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #dc2626;'>Mật khẩu của bạn đã được thay đổi</h2>
                    <p>Mật khẩu tài khoản TLUMeet của bạn vừa được thay đổi lúc <strong>{changedAt}</strong>.</p>
                    <p>Nếu bạn là người thực hiện thay đổi này, bạn có thể bỏ qua email này.</p>
                    <p style='color:#dc2626;'><strong>Nếu bạn KHÔNG thực hiện thay đổi này</strong>, vui lòng đặt lại mật khẩu ngay
                       và liên hệ quản trị viên để bảo vệ tài khoản.</p>
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                    <p style='color:#6b7280;font-size:12px;'>Email này được gửi tự động, vui lòng không trả lời.</p>
                </div>";

            await emailService.SendEmailAsync(
                message.Email,
                "Cảnh báo: mật khẩu TLUMeet của bạn đã được thay đổi",
                emailBody,
                "PasswordChanged");
        }
    }
}
