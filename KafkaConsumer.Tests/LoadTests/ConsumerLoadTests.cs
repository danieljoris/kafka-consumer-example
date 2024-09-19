using NBomber.CSharp;
using NBomber.Contracts;
using Microsoft.Extensions.DependencyInjection;
using KafkaConsumer.Tests;
using KafkaConsumer.Worker;

public class ConsumerLoadTests
{
    public static void Run()
    {
        // Obtenha o ServiceProvider configurado para testes
        var serviceProvider = TestServiceProvider.CreateServiceProvider();

        var consumer = serviceProvider.GetService<Consumer>();

        var scenario = Scenario.Create("load_test_consumer", async context =>
        {
            // Define o passo de teste: simula o consumo de uma mensagem
            var step1 = await Step.Run("consume_message", context,  async () =>
            {
                try
                {
                    await consumer.StartAsync();
                    return Response.Ok();
                }
                catch (Exception)
                {
                    return Response.Fail();
                }
            });

            return Response.Ok();
        }).WithLoadSimulations(
            // Configura o cenário de carga com múltiplas execuções do Consumer
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(4), during: TimeSpan.FromMinutes(5))
        );

        // Executa o teste de carga
        NBomberRunner.RegisterScenarios(scenario).Run();
    }
}
