
using NotificationService.API.Events;
using NotificationService.API.Services;

namespace NotificationService.API.Consumers
{
    public class WelcomeEmailConsumer : KafkaConsumerBase<UserRegisteredEvent>
    {
        private readonly IServiceProvider _serviceProvider;
        protected override string Topic => "user-registered-events";
        protected override string GroupId => "notification-service-welcome";

        public WelcomeEmailConsumer(IConfiguration configuration,ILogger<WelcomeEmailConsumer> logger, IServiceProvider serviceProvider) : base(configuration, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessMessageAsync(UserRegisteredEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #2563eb;'>Chào mừng đến với TLUMeet!</h2>
                <p>Xin chào <strong>{message.UserName}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản TLUMeet.</p>
                <p>Với TLUMeet, bạn có thể:</p>
                <ul>
                    <li>Tạo và tham gia các cuộc họp video</li>
                    <li>Lên lịch các cuộc họp trong tương lai</li>
                    <li>Quản lý participants</li>
                    <li>Và nhiều tính năng khác...</li>
                </ul>
                <p>Hãy bắt đầu tạo cuộc họp đầu tiên của bạn!</p>
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                <p style='color: #6b7280; font-size: 12px;'>
                    Email này được gửi tự động, vui lòng không trả lời.
                </p>
            </div>
        ";

            await emailService.SendEmailAsync(
                message.Email,
                "Chào mừng đến với TLUMeet",
                emailBody,
                "Welcome");
        }
    }
}
