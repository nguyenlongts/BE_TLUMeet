using NotificationService.API.Events;
using NotificationService.API.Services;

namespace NotificationService.API.Consumers;

public class PasswordResetConsumer : KafkaConsumerBase<PasswordResetEvent>
{
    private readonly IServiceProvider _serviceProvider; 
    private readonly IConfiguration _configuration;


    protected override string Topic => "password-reset-events";
    protected override string GroupId => "notification-service-password-reset";

    public PasswordResetConsumer(IConfiguration configuration,ILogger<PasswordResetConsumer> logger,IServiceProvider serviceProvider): base(configuration, logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessMessageAsync(PasswordResetEvent message)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var fe = _configuration["FE:BaseUrl"];

        var resetLink = $"{fe}/reset-password?token={message.ResetToken}";

        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #dc2626;'>Đặt lại mật khẩu</h2>
                <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản TLUMeet.</p>
                <p>Nhấn vào nút bên dưới để đặt lại mật khẩu:</p>
                <p style='margin: 30px 0;'>
                    <a href='{resetLink}' 
                       style='background-color: #dc2626; color: white; padding: 12px 24px; 
                              text-decoration: none; border-radius: 6px;'>
                        Đặt lại mật khẩu
                    </a>
                </p>
                <p style='color: #ef4444;'><strong>Link này có hiệu lực đến {message.ExpiresAt} .</strong></p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                <p style='color: #6b7280; font-size: 12px;'>
                    Nếu nút không hoạt động, copy link sau vào trình duyệt:<br>
                    {resetLink}
                </p>
            </div>
        ";

        await emailService.SendEmailAsync(
            message.Email,
            "Đặt lại mật khẩu TLUMeet",
            emailBody);
    }
}