namespace MeetingService.Infrastructure.Messaging;

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, T message, string key) where T : class;
}

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, string key) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = key,
                Value = json
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage);
            _logger.LogInformation("Published to {Topic} at offset {Offset}", topic, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing to {Topic}", topic);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}

