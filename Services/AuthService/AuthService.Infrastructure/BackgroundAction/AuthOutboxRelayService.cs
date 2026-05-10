using AuthService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.BackgroundAction
{
    public class AuthOutboxRelayService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly ILogger<AuthOutboxRelayService> _logger;
        public AuthOutboxRelayService(IServiceScopeFactory serviceFactory, ILogger<AuthOutboxRelayService> logger)
        {
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try {
                    await ProcessMessages();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi OutboxRelay");
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            }
        }

        private async Task ProcessMessages()
        {
            using var scoped = _serviceFactory.CreateScope();
            var context = scoped.ServiceProvider.GetRequiredService<AuthDbContext>();
            var producer = scoped.ServiceProvider.GetRequiredService<IKafkaProducer>();
            var messages = await context.OutboxMessages.Where(m=>m.OccuredAt == null).OrderBy(m => m.CreatedAt).Take(20).ToListAsync();
            foreach (var message in messages)
            {
                try
                {
                    switch (message.EventType) {
                        case nameof(UserRegisteredEvent):
                            var userCreatedEvent = System.Text.Json.JsonSerializer.Deserialize<UserRegisteredEvent>(message.Payload);
                            await producer.PublishAsync(KafkaTopics.UserRegistered, userCreatedEvent!);
                            break;
                        case nameof(PasswordResetRequestedEvent):
                            var passwordResetRequestedEvent = System.Text.Json.JsonSerializer.Deserialize<PasswordResetRequestedEvent>(message.Payload);
                          
                            await producer.PublishAsync(KafkaTopics.PasswordResetRequested, passwordResetRequestedEvent!);
                            break; 
                }
                    message.OccuredAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi publish message {Id}", message.Id);
                    message.ErrorMessage = ex.Message;
                }
            }
            await context.SaveChangesAsync();

        }
    }
}