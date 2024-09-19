using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaConsumer.Worker;
using KafkaConsumer.Worker.Services;
using KafkaConsumer.Worker.Services.External;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace KafkaConsumer.Tests
{
    public class TestServiceProvider
    {
        public static ServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            // Usar MemoryDistributedCache no lugar do Redis para os testes
            services.AddDistributedMemoryCache(); // Simula cache em memória

            // Configurando o rate limiter para os testes com cache em memória
            services.AddScoped<IRateLimiter>(provider =>
            {
                var memoryCache = provider.GetRequiredService<IDistributedCache>();
                return new RedisFixedWindowRateLimiter(memoryCache, 5, TimeSpan.FromSeconds(10)); // Limite e janela de tempo
            });

            // Mock do IExternalBankService para simular comportamento
            var mockExternalBankService = new Mock<IExternalBankService>();
            mockExternalBankService.Setup(s => s.CheckAvailableValueAsync(It.IsAny<CheckAvailableValueRequest>()))
                .ReturnsAsync(new CheckAvailableValueResult());
            services.AddSingleton(mockExternalBankService.Object);

            // Adiciona políticas para Retry, Rate Limit e Circuit Breaker
            services.AddHttpClient<IExternalBankService, ExternalBankService>()
                .AddPolicyHandler((serviceProvider, request) =>
                {
                    var rateLimiter = serviceProvider.GetRequiredService<IRateLimiter>();
                    return CustomPolicies.RateLimitPolicy(rateLimiter);
                })
                .AddPolicyHandler(CustomPolicies.RetryPolicy())
                .AddPolicyHandler(CustomPolicies.CircuitBreakerPolicy());

            // Adiciona o Consumer para testes
            services.AddScoped<Consumer>();

            // Retorna o service provider configurado para os testes
            return services.BuildServiceProvider();
        }
    }
}
