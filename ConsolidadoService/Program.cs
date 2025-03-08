using ConsolidadoService.Consumer;
using MassTransit;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SharedLib.Models;
using SharedLib.Settings;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        BsonSerializer.RegisterSerializer<DateOnly>(new DateOnlySerializer());

        builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetSection("MongoDbSettings")
        );

        builder.Services.Configure<MessagingSettings>(
            builder.Configuration.GetSection("MessagingSettings")
        );

        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var mongoOptions = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var clientSettings = MongoClientSettings.FromConnectionString(mongoOptions.ConnectionString);
            return new MongoClient(clientSettings);
        });

        // Standard approach for .NET 8+ OpenAPI (Swagger)
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ConsolidadoService API", Version = "v1" });
        });

        var messagingSettings = builder.Configuration
            .GetSection("MessagingSettings")
            .Get<MessagingSettings>();

        if (messagingSettings == null)
            throw new InvalidOperationException("MessagingSettings not found");

        if (messagingSettings.Provider.Equals("RabbitMQ", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<LancamentoRealizadoConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    var opts = context.GetRequiredService<IOptions<MessagingSettings>>().Value;
                    var fullHost = $"{opts.Prefix}{opts.HostUri}:{opts.Port}";

                    cfg.Host(new Uri(fullHost), h =>
                    {
                        h.Username(opts.Username);
                        h.Password(opts.Password);
                    });

                    cfg.ReceiveEndpoint("lancamentos-realizados", e =>
                    {
                        e.ConfigureConsumer<LancamentoRealizadoConsumer>(context);
                    });
                });
            });
        }

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapGet("/consolidado", async (
            IMongoClient mongoClient,
            IOptions<MongoDbSettings> mongoDbOpts,
            DateOnly? date
        ) =>
        {
            var databaseName = mongoDbOpts.Value.DatabaseName ?? "FinanceiroDB";
            var database = mongoClient.GetDatabase(databaseName);
            var collection = database.GetCollection<ConsolidadoDiario>("ConsolidadoDiario");

            var dateToUse = date ?? DateOnly.FromDateTime(DateTime.Today);

            var result = await collection
                .Find(x => x.Data == dateToUse)
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return Results.Ok(new
                {
                    Data = dateToUse,
                    TotalCreditos = 0m,
                    TotalDebitos = 0m,
                    Saldo = 0m
                });
            }

            return Results.Ok(new
            {
                result.Data,
                result.TotalCreditos,
                result.TotalDebitos,
                Saldo = result.Saldo
            });
        });

        app.Run();
    }
}
