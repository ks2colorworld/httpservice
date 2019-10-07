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
            RequestModel requestModel = new RequestModel();
            Dictionary<string, string> dic = requestModel;
            string selectedDictionary = nameof(requestModel);
            string propertyName = String.Empty;

            while (reader.Read())
            {
                if(reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();

                    if (propertyName.Equals("data", StringComparison.OrdinalIgnoreCase))
                    {
                        selectedDictionary = nameof(requestModel.Data);
                        dic = requestModel.Data;
                    }   
                }

                if(reader.TokenType == JsonTokenType.EndObject)
                {
                    if (selectedDictionary.Equals(nameof(requestModel.Data)))
                    {
                        selectedDictionary = nameof(requestModel);
                        dic = requestModel;
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

            return requestModel;
        }

        public override void Write(Utf8JsonWriter writer, RequestModel value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }
}
