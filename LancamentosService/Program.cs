using Microsoft.Extensions.Options;
using MassTransit;
using MongoDB.Driver;
using SharedLib.Models;
using SharedLib.Events;
using SharedLib.Settings;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Bind "MongoDbSettings"
        builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetSection("MongoDbSettings")
        );

        // Register MongoClient
        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var mongoOptions = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(mongoOptions.ConnectionString);
        });

        // Bind "MessagingSettings"
        builder.Services.Configure<MessagingSettings>(
            builder.Configuration.GetSection("MessagingSettings")
        );

        // Conditionally add MassTransit if provider is "RabbitMQ"
        var messagingProvider = builder.Configuration.GetValue<string>("MessagingSettings:Provider");
        if (messagingProvider?.Equals("RabbitMQ", StringComparison.OrdinalIgnoreCase) == true)
        {
            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var messagingOpts = context.GetRequiredService<IOptions<MessagingSettings>>().Value;
                    var fullHost = $"{messagingOpts.Prefix}{messagingOpts.HostUri}:{messagingOpts.Port}";
                    cfg.Host(new Uri(fullHost), h =>
                    {
                        h.Username(messagingOpts.Username);
                        h.Password(messagingOpts.Password);
                    });
                });
            });
        }

        // Add standard .NET 8 approach for Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Enable Swagger in development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Minimal POST endpoint
        app.MapPost("/lancamentos", async (
            Lancamento lancamento,
            IMongoClient mongoClient,
            IPublishEndpoint publishEndpoint,
            IOptions<MongoDbSettings> mongoDbOptions) =>
        {
            var dbName = mongoDbOptions.Value.DatabaseName;
            var database = mongoClient.GetDatabase(dbName);
            var collection = database.GetCollection<Lancamento>("Lancamentos");

            await collection.InsertOneAsync(lancamento);

            await publishEndpoint.Publish(new LancamentoRealizadoEvent
            {
                Id = lancamento.Id,
                Data = lancamento.Data,
                Valor = lancamento.Valor,
                Tipo = lancamento.Tipo
            });

            return Results.Created($"/lancamentos/{lancamento.Id}", lancamento);
        });

        app.Run();
    }
}