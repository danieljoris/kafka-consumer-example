using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace KafkaConsumer.Worker.Services
{
    public class RedisFixedWindowRateLimiter : IRateLimiter
    {
        private readonly IDistributedCache _cache;
        private readonly FixedWindowRateLimiterOptions _limiterOptions;
        private readonly TimeSpan _window;

        public RedisFixedWindowRateLimiter(IDistributedCache cache, int limit, TimeSpan window)
        {
            _cache = cache;
            _limiterOptions = new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit,
                Window = window,
                QueueLimit = 0,
                AutoReplenishment = false,
            };
            _window = window;
        }

        public async Task<bool> CanMakeRequestAsync(string key)
        {
            var redisKey = $"rate-limit:{key}";
            var currentCount = await _cache.GetStringAsync(redisKey);

            // Se não há contagem ainda, cria uma nova janela
            if (currentCount == null)
            {
                // Inicializa um novo rate limiter localmente
                var limiter = new FixedWindowRateLimiter(_limiterOptions);

                // Primeira requisição, atualiza o cache do Redis com o valor "1"
                await _cache.SetStringAsync(redisKey, "1", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _window // Expira no fim da janela
                });

                // Permite a requisição
                return true;
            }

            int count = int.Parse(currentCount);

            // Recupera o limite de requisições da janela atual
            if (count >= _limiterOptions.PermitLimit)
            {
                // Se o limite foi atingido, bloqueia a requisição
                return false;
            }

            // Incrementa o contador no Redis
            count++;
            await _cache.SetStringAsync(redisKey, count.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _window // Expira ao final da janela
            });

            return true;
        }

    }
}
