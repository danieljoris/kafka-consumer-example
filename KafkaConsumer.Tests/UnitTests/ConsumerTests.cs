using Xunit;
using Microsoft.Extensions.DependencyInjection;
using KafkaConsumer.Tests;
using KafkaConsumer.Worker.Services.External;
using KafkaConsumer.Worker.Services;
using KafkaConsumer.Worker;
using Moq;

public class ConsumerUnitTests
{
    private readonly IExternalBankService _externalBankService;
    private readonly IRateLimiter _rateLimiter;
    private readonly Consumer _consumer;

    public ConsumerUnitTests()
    {
        // Obtenha o ServiceProvider configurado para testes
        var serviceProvider = TestServiceProvider.CreateServiceProvider();

        _externalBankService = serviceProvider.GetService<IExternalBankService>();
        _rateLimiter = serviceProvider.GetService<IRateLimiter>();

        _consumer = new Consumer(_externalBankService, _rateLimiter);
    }

    [Fact]
    public async Task Should_Process_Request_When_RateLimiter_Allows()
    {
        // Act
        await _consumer.StartAsync();

        // Assert
        Assert.NotNull(_externalBankService);
        Assert.NotNull(_rateLimiter);
        // Verifica se o método CheckAvailableValueAsync foi chamado uma vez
        Mock.Get(_externalBankService).Verify(s => s.CheckAvailableValueAsync(It.IsAny<CheckAvailableValueRequest>()), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Process_Request_When_RateLimiter_Denies()
    {
        // Arrange: customiza o mock do rate limiter para retornar false
        Mock.Get(_rateLimiter).Setup(rl => rl.CanMakeRequestAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _consumer.StartAsync();

        // Assert
        Mock.Get(_externalBankService).Verify(s => s.CheckAvailableValueAsync(It.IsAny<CheckAvailableValueRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_Handle_ExternalService_Exception()
    {
        // Arrange: customiza o mock para lançar uma exceção quando chamado
        Mock.Get(_externalBankService).Setup(s => s.CheckAvailableValueAsync(It.IsAny<CheckAvailableValueRequest>())).ThrowsAsync(new HttpRequestException());

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () => await _consumer.StartAsync());
    }
}
