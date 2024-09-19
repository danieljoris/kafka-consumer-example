using KafkaConsumer.Worker.Services;
using KafkaConsumer.Worker.Services.External;
using Polly;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KafkaConsumer.Worker
{
    public class Consumer
    {
        private readonly IExternalBankService _externalBankService;
        private readonly IRateLimiter _rateLimiter;
        private int _requestCounter; // Contador de requisições
        private readonly Stopwatch _stopwatch; // Cronômetro para medir o tempo de 1 segundo

        public Consumer(IExternalBankService externalBankService, IRateLimiter rateLimiter)
        {
            _externalBankService = externalBankService;
            _rateLimiter = rateLimiter;
            _requestCounter = 0;
            _stopwatch = new Stopwatch();
            _stopwatch.Start(); // Inicia o cronômetro ao iniciar o Consumer
        }

        public async Task StartAsync()
        {
            var tasks = new List<Task>();

            while (true)
            {
                // Verifica se pode fazer a requisição antes de adicioná-la às tarefas
                if (await _rateLimiter.CanMakeRequestAsync("global-rate-limit"))
                {
                    var request = await ConsumeFakeKafkaMessageAsync();

                    tasks.Add(ProcessRequestAsync(request));

                    _requestCounter++; // Incrementa o contador de requisições
                }

                // Verifica se o cronômetro atingiu 1 segundo
                if (_stopwatch.ElapsedMilliseconds >= 1000)
                {
                    // Espera todas as requisições paralelas serem concluídas
                    await Task.WhenAll(tasks);

                    // Só imprime se o contador for maior que zero
                    if (_requestCounter > 0)
                    {
                        // Log para monitorar quantas requisições foram feitas dentro do período de 1 segundo
                        Console.WriteLine($"Total de requisições em 1 segundo: {_requestCounter}");
                    }

                    // Reseta o cronômetro e o contador
                    _stopwatch.Restart();
                    _requestCounter = 0;

                    tasks.Clear(); // Limpa a lista de tarefas após a execução
                }
            }
        }


        private async Task ProcessRequestAsync(CheckAvailableValueRequest request)
        {
            // Exibe a requisição individual
            Console.WriteLine($"Requisição {_requestCounter} iniciada");
            var requestId = _requestCounter;

            // Realiza a requisição ao serviço externo
            var response = await _externalBankService.CheckAvailableValueAsync(request);

            // Exibe que a requisição foi finalizada
            Console.WriteLine($"Requisição {requestId} finalizada dentro de {_stopwatch.ElapsedMilliseconds}ms");
        }

        private Task<CheckAvailableValueRequest> ConsumeFakeKafkaMessageAsync()
        {
            var simulatedMessage = new CheckAvailableValueRequest
            {
                Cpf = "12345678900", // Simula um CPF válido
                BirthDate = new DateTime(1990, 1, 1) // Simula uma data de nascimento
            };

            return Task.FromResult(simulatedMessage);
        }
    }
}
