using Xunit;
using Microsoft.Extensions.DependencyInjection;
using KafkaConsumer.Tests;
using KafkaConsumer.Worker.Services.External;
using KafkaConsumer.Worker.Services;
using KafkaConsumer.Worker;
using Moq;
using System.Threading.RateLimiting;

public class ConsumerIntegrationTests
{
    private readonly Consumer _consumer;
    private readonly IExternalBankService _externalBankService;
    private readonly IRateLimiter _rateLimiter;

    public ConsumerIntegrationTests()
    {
        // Obtenha o ServiceProvider configurado para testes
        var serviceProvider = TestServiceProvider.CreateServiceProvider();

        _externalBankService = serviceProvider.GetService<IExternalBankService>();
        _rateLimiter = serviceProvider.GetService<IRateLimiter>();

        _consumer = new Consumer(_externalBankService, _rateLimiter);
    }

    [Fact]
    public async Task Should_Process_Multiple_Requests_With_RateLimiting()
    {
        for (int i = 0; i < 10; i++)
        {
            await _consumer.StartAsync();
        }

        // Simula e verifica se o rate limiter aplicou o limite de requisições
        Mock.Get(_rateLimiter).Verify(rl => rl.CanMakeRequestAsync(It.IsAny<string>()), Times.AtLeast(5));
    }

    [Fact]
    public async Task Should_Honor_RateLimit()
    {
        // Arrange: customiza o mock para permitir apenas 5 requisições
        Mock.Get(_rateLimiter).SetupSequence(rl => rl.CanMakeRequestAsync(It.IsAny<string>()))
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(false); // Bloqueia a 6ª requisição

        for (int i = 0; i < 6; i++)
        {
            await _consumer.StartAsync();
        }

        // Assert: Verifica se o serviço externo foi chamado 5 vezes e bloqueado na 6ª
        Mock.Get(_externalBankService).Verify(s => s.CheckAvailableValueAsync(It.IsAny<CheckAvailableValueRequest>()), Times.Exactly(5));
        Mock.Get(_rateLimiter).Verify(rl => rl.CanMakeRequestAsync(It.IsAny<string>()), Times.Exactly(6));
    }
}
