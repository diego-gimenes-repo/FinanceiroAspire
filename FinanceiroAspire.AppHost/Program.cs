using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);
        var mongo = builder.AddMongoDB("mongo")
                   .WithDataVolume();

        var mongodb = mongo.AddDatabase("FinanceiroDB");
        // MongoDB Container
       
        var rabbitmq = builder.AddRabbitMQ("rabbitmq").WithManagementPlugin(); ;
  

        // Lancamentos Service
        var lancamentosService = builder.AddProject<Projects.LancamentosService>("lancamentos-service")
            .WithReference(mongo)
            .WithReference(rabbitmq)
            .WithHttpEndpoint(port: 5001, name: "lancamentos-http");

        // Consolidado Service
        var consolidadoService = builder.AddProject<Projects.ConsolidadoService>("consolidado-service")
            .WithReference(mongo)
            .WithReference(rabbitmq)
            .WithHttpEndpoint(port: 5002, name: "consolidado-http");

        builder.Build().Run();
    }
}
