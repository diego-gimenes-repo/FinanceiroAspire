
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;

namespace SharedLib.Serialization
{
    public class DateOnlySerializer : StructSerializerBase<DateOnly>
    {
        // We will treat each DateOnly as midnight UTC for storage.
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
        {
            var dateTime = value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            // The MongoDB C# driver stores DateTime as milliseconds since Unix epoch by default.
            context.Writer.WriteDateTime(dateTime.ToUniversalTime().Ticks);
        }

        public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Read the stored date ticks, then convert it back to a UTC DateTime
            var ticks = context.Reader.ReadDateTime();
            var dateTime = new DateTime(ticks, DateTimeKind.Utc);
            return DateOnly.FromDateTime(dateTime);
        }
    }
}
