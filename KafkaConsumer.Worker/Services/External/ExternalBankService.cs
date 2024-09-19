using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KafkaConsumer.Worker.Services.External
{
    public class ExternalBankService : IExternalBankService
    {

        private string _baseUrl { get; init; } = "https://mocki.io/v1/";

        private readonly HttpClient _httpClient;

        public ExternalBankService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<CheckAvailableValueResult?> CheckAvailableValueAsync(CheckAvailableValueRequest request)
        {
            var response = await _httpClient.GetAsync("d19656cf-4831-4bc5-a57b-8e45b14ffedb");
            response.EnsureSuccessStatusCode();

            string responseData = await response.Content.ReadAsStringAsync();
            var result = Deserialize<CheckAvailableValueResult>(responseData);
            await Task.Delay(new Random().Next(1, 50));

            return result;
        }


        private T? Deserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }
    }
}
