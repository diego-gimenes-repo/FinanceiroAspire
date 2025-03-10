using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7135") 
    };

    static async Task Main(string[] args)
    {
        int totalRequests = 1000;  // Total de requisições a serem testadas
        int concurrencyLevel = 50; // Número de requisições simultâneas

        Console.WriteLine($"Iniciando teste de carga com {totalRequests} requisições e {concurrencyLevel} em paralelo...");

        var stopwatch = Stopwatch.StartNew();

        using (var semaphore = new SemaphoreSlim(concurrencyLevel))
        {
            var tasks = new Task[totalRequests];

            for (int i = 0; i < totalRequests; i++)
            {
                await semaphore.WaitAsync();
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        await SendRequest();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }

            await Task.WhenAll(tasks);
        }

        stopwatch.Stop();

        double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
        double rps = totalRequests / elapsedSeconds;

        Console.WriteLine($"\nTeste concluído em {elapsedSeconds:F2} segundos.");
        Console.WriteLine($"Taxa de requisições por segundo (RPS): {rps:F2}");
    }

    private static async Task SendRequest()
    {
        try
        {
            var response = await _httpClient.GetAsync("/consolidado");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na requisição: {ex.Message}");
        }
    }
}
