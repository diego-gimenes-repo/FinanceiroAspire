using MongoDB.Bson.Serialization.Attributes;

namespace SharedLib.Models
{
    [BsonIgnoreExtraElements]
    public class ConsolidadoDiario
    {
        public DateOnly Data { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal TotalDebitos { get; set; }
        public decimal Saldo => TotalCreditos - TotalDebitos;
    }
}
