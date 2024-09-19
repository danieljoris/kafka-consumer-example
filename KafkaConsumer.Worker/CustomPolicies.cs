using KafkaConsumer.Worker.Services;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaConsumer.Worker
{
    public class CustomPolicies
    {

        public static IAsyncPolicy<HttpResponseMessage> RateLimitPolicy(IRateLimiter rateLimiter)
        {
            return Policy<HttpResponseMessage>
                .HandleResult(response => response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .Or<Exception>() // Ou outras exceções que podem acontecer
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                async (result, timespan, retryCount, context) =>
                {
                    // Verificar se o rate limiter permite requisições
                    while (!await rateLimiter.CanMakeRequestAsync("global-rate-limit"))
                    {
                        Console.WriteLine("Rate limit atingido. Aguardando...");
                        await Task.Delay(500); // Aguardar até que o rate limit permita novas requisições
                    }

                    Console.WriteLine($"Tentativa {retryCount} falhou. Tentando novamente em {timespan.TotalSeconds} segundos...");
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30),
                    onBreak: (outcome, timespan) =>
                    {
                        Console.WriteLine("Circuito aberto! Não faremos novas requisições por 30 segundos.");
                    },
                    onReset: () => Console.WriteLine("Circuito fechado! Voltando a aceitar requisições."));
        }

        public static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Tentativa {retryAttempt} falhou. Tentando novamente em {timespan}.");
                    });
        }
    }
}
