using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaConsumer.Worker.Services
{
    /// <summary>
    /// Interface genérica para controle de rate limiting.
    /// Permite a implementação de diferentes estratégias de limitação de requisições,
    /// possibilitando o uso de diferentes backends, como Redis, bancos de dados, etc.
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Verifica se uma requisição pode ser feita com base no limite configurado.
        /// </summary>
        /// <param name="key">Chave única que identifica o conjunto de requisições a ser limitado.</param>
        /// <returns>Retorna true se a requisição puder ser feita, ou false se o limite foi atingido.</returns>
        Task<bool> CanMakeRequestAsync(string key);
    }

}
