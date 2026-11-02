using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZurichClaims.Api.Converters;

public class StrictJsonStringEnumConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!typeToConvert.IsEnum)
        {
            return null;
        }

        var converterType = typeof(StrictEnumConverterInner<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private class StrictEnumConverterInner<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"No se permite un valor no textual para el enum {typeof(TEnum).Name}.");
            }

            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue) || char.IsDigit(stringValue[0]) || stringValue[0] == '-' || stringValue[0] == '+')
            {
                throw new JsonException($"El valor '{stringValue}' no es un nombre válido para el enum {typeof(TEnum).Name}.");
            }

            if (Enum.TryParse<TEnum>(stringValue, ignoreCase: true, out var result) && Enum.IsDefined(typeof(TEnum), result))
            {
                return result;
            }

            throw new JsonException($"El valor '{stringValue}' no es válido para {typeof(TEnum).Name}.");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
