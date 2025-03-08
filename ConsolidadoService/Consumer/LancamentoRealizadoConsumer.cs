using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SharedLib.Events;
using SharedLib.Models;
using SharedLib.Settings;

namespace ConsolidadoService.Consumer
{
    public class LancamentoRealizadoConsumer : IConsumer<LancamentoRealizadoEvent>
    {
        private readonly IMongoCollection<ConsolidadoDiario> _collection;

        public LancamentoRealizadoConsumer(
            IMongoClient client,
            IOptions<MongoDbSettings> mongoDbOptions)
        {
            var databaseName = mongoDbOptions.Value.DatabaseName;
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<ConsolidadoDiario>("ConsolidadoDiario");
        }

        public async Task Consume(ConsumeContext<LancamentoRealizadoEvent> context)
        {
            var evento = context.Message;
            var dateOnly = DateOnly.FromDateTime(evento.Data);
            var filter = Builders<ConsolidadoDiario>.Filter.Eq(x => x.Data, dateOnly);
            var fieldToIncrement = evento.Tipo == TipoLancamento.Credito
                ? nameof(ConsolidadoDiario.TotalCreditos)
                : nameof(ConsolidadoDiario.TotalDebitos);
            var update = Builders<ConsolidadoDiario>.Update.Inc(fieldToIncrement, evento.Valor);
            await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }
    }
}
