using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dockhand.Utils
{
    public class DockerPercentStringConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal) || objectType == typeof(decimal?));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {           
            var token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Float:
                case JTokenType.Integer:
                    return token.ToObject<decimal>();
                case JTokenType.String:
                {
                    var value = token
                        .ToString()
                        .Trim()
                        .TrimEnd('%');

                    if (!decimal.TryParse(value, out var result))
                    {
                        throw new JsonSerializationException($"Invalid decimal string format");
                    };

                    return result;
                }
                case JTokenType.Null when objectType == typeof(decimal?):
                    return null;
                default:
                    throw new JsonSerializationException($"Unexpected token type: {token.Type}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
