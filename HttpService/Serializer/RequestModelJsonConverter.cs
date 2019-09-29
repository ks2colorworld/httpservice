using HttpService.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpService.Serializer
{
    public class RequestModelJsonConverter : JsonConverter<RequestModel>
    {
        public override RequestModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            RequestModel data = new RequestModel();
            Dictionary<string, string> dic = data;
            string selectedDictionary = nameof(data);
            string propertyName = String.Empty;

            while (reader.Read())
            {
                if(reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();

                    if (propertyName.Equals("parameters", StringComparison.OrdinalIgnoreCase))
                    {
                        selectedDictionary = nameof(data.Parameters);
                        dic = data.Parameters;
                    }                    
                }

                if(reader.TokenType == JsonTokenType.EndObject)
                {
                    if (selectedDictionary.Equals(nameof(data.Parameters)))
                    {
                        selectedDictionary = nameof(data);
                        dic = data;
                    }                    
                }

                if(reader.TokenType == JsonTokenType.String)
                {
                    if (!String.IsNullOrWhiteSpace(propertyName))
                    {
                        dic.Add(propertyName, reader.GetString());
                    }
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    if (!String.IsNullOrWhiteSpace(propertyName))
                    {
                        dic.Add(propertyName, reader.GetString());
                    }
                }
            }

            return data;
        }

        public override void Write(Utf8JsonWriter writer, RequestModel value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }
}
