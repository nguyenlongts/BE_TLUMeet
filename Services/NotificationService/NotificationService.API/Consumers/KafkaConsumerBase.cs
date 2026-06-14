using Confluent.Kafka;
using System.Text.Json;

public abstract class KafkaConsumerBase<T> : BackgroundService where T : class
{
    private IConsumer<string, string> _consumer;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    protected abstract string Topic { get; }
    protected abstract string GroupId { get; }

    protected KafkaConsumerBase(IConfiguration configuration, ILogger logger)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = GroupId,
                // Earliest: nhóm consumer mới (topic vừa tạo) đọc từ đầu, tránh bỏ lỡ message đầu tiên.
                // Nhóm đã có offset commit sẽ tiếp tục từ offset đó (auto.offset.reset chỉ áp dụng khi chưa có offset).
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(Topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Consumer}] FAILED during setup for topic {Topic}", GetType().Name, Topic);
            return;
        }

        await Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? consumeResult = null;
                try
                {
                    consumeResult = _consumer.Consume(stoppingToken);
                    var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);
                    if (message != null)
                    {
                        await ProcessMessageAsync(message);
                        _logger.LogInformation("[{Consumer}] Processed message from topic {Topic}", GetType().Name, Topic);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (ConsumeException ex)
                {
                    // ✅ Tách riêng: topic chưa tồn tại thì KHÔNG commit, chờ rồi retry
                    _logger.LogWarning("[{Consumer}] Topic not available: {Reason}. Retrying in 5s...",
                        GetType().Name, ex.Error.Reason);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{Consumer}] Error on topic {Topic}", GetType().Name, Topic);
                    // ✅ Chỉ commit khi thực sự đã consume được message
                    if (consumeResult != null)
                        _consumer.Commit(consumeResult);
                }
            }
        }, stoppingToken);
    }

    protected abstract Task ProcessMessageAsync(T message);

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}