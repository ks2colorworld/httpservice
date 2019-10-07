using HttpService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpService.Serializer
{
    public class XmlSerializer : ISerializer
    {
        private const int space = 4;
        private const string FIRST_LINE = "<? xml version=\"1.0\" encoding=\"utf-8\"?>";

        public string Serialize(ResponseModel model)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(FIRST_LINE);
            builder.Append("<value");

            if (!String.IsNullOrWhiteSpace(model.Values.Namespace))
            {
                builder.Append($" namespace=\"{model.Values.Namespace}\"");
            }
            builder.AppendLine(">");
            builder.Append(WriteTable(model.Values));
            builder.AppendLine("</value>");

            return builder.ToString();
        }

        private string WriteTable(Dictionary<string , IEnumerable<Dictionary<string, object>>> tables, int indent = 1)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var table in tables)
            {
                if (table.Value.Count() == 0)
                {
                    builder.Append("".PadLeft(indent * space));
                    builder.AppendLine($"<{table.Key} />");
                }
                else
                {
                    foreach (var items in table.Value)
                    {
                        builder.Append("".PadLeft(indent * space));
                        builder.AppendLine($"<{table.Key}>");

                        builder.Append(WriteDictionary(items, indent + 1));

                        builder.Append("".PadLeft(indent * space));
                        builder.AppendLine($"</{table.Key}>");
                    }
                }
            }

            return builder.ToString();
        }

        private string WriteDictionary(Dictionary<string, object> dictionary, int indent = 1)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var item in dictionary)
            {
                if (item.Value == null)
                {
                    builder.Append("".PadLeft(indent * space));
                    builder.AppendLine($"<{item.Key} />");
                }
                else
                {
                    builder.Append("".PadLeft(indent * space));
                    builder.Append($"<{item.Key}>");
                    builder.Append($"{item.Value}");
                    builder.AppendLine($"</{item.Key}>");
                }
            }

            return builder.ToString();
        }
    }
}
