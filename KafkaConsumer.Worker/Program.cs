using KafkaConsumer.Worker;
using KafkaConsumer.Worker.Services;
using KafkaConsumer.Worker.Services.External;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;




var services = new ServiceCollection();

// Injeção do Redis para cache distribuído
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Endereço do Redis
});


// Configurando o rate limiter com limite de 5 requisições por 10 segundo
services.AddScoped<IRateLimiter>(provider =>
{
    var redisCache = provider.GetRequiredService<IDistributedCache>();
    return new RedisFixedWindowRateLimiter(redisCache, 5, TimeSpan.FromSeconds(10)); // Limite e janela de tempo
});

services.AddHttpClient<IExternalBankService, ExternalBankService>()
    .AddPolicyHandler((serviceProvider, request) =>
    {
        var rateLimiter = serviceProvider.GetRequiredService<IRateLimiter>();
        return CustomPolicies.RateLimitPolicy(rateLimiter);
    })
    .AddPolicyHandler(CustomPolicies.RetryPolicy()) // Adiciona política de Retry
    .AddPolicyHandler(CustomPolicies.CircuitBreakerPolicy()); // Adiciona política de Circuit Breaker;

services.AddScoped<Consumer>();


var serviceProvider = services.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var consumer = scope.ServiceProvider.GetRequiredService<Consumer>();
    await consumer.StartAsync();
}