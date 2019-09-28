using HttpService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpService.Serializer
{
    public class JsonSerializer : ISerializer
    {
        private const int space = 4;
        public string Serialize(ResponseModel model)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("{");
            builder.Append("".PadLeft(1 * space));
            builder.Append("\"values\": [");

            builder.Append(WriteTable(model.Values, 2));
            
            builder.Append("".PadLeft(1 * space));
            builder.Append("]");
            builder.AppendLine("}");

            return builder.ToString();
        }

        public string WriteTable(Dictionary<string, IEnumerable<Dictionary<string, object>>> tables, int indent = 1)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var table in tables)
            {
                builder.Append("".PadLeft(indent * space));
                builder.AppendLine($"\"{table.Key}\": [");

                //builder.Append(WriteDictionary(item.Value), indent + 1));

                foreach (var item in table.Value)
                {
                    builder.Append(WriteDictionary(item, indent + 1));
                    builder.Append(",");
                }

                if (builder.Length > 1)
                {
                    builder.Remove(builder.Length - 1, 1);
                }

                builder.Append("".PadLeft(indent * space));
                builder.AppendLine("],");
            }
            if (builder.Length > 1)
            {
                builder.Remove(builder.Length - 1, 1);
            }

            return builder.ToString();
        }

        public string WriteDictionary(Dictionary<string, object> dictionary, int indent = 1)
        {
            if(dictionary == null || dictionary.Count == 0)
            {
                return String.Empty;
            }

            StringBuilder builder = new StringBuilder();

            builder.Append("".PadLeft(indent * space));
            builder.AppendLine("{");
            foreach (var item in dictionary)
            {
                builder.Append("".PadLeft((indent + 1) * space));
                builder.Append($"\"{item.Key}\": ");
                builder.AppendLine($"{ (item.Value == null ? "null" : $"\"{item.Value}\"")},");
            }
            if (builder.Length > 1)
            {
                builder.Remove(builder.Length - 1, 1);
            }
            builder.Append("".PadLeft(indent * space));
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
