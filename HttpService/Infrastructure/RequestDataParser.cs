using HttpService.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpService
{
    public class RequestDataParser
    {
        public RequestDataParser(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            context = httpContextAccessor.HttpContext;
        }

        public async Task<RequestModel> Parse()
        {
            RequestModel model = null;
            if (context.Request.Method.ToLower() == "post")
            {
                var data = String.Empty;
                using (var reader = new StreamReader(context.Request.Body))
                {
                    data = await reader.ReadToEndAsync();
                    reader.Close();
                }


                if (!String.IsNullOrWhiteSpace(data))
                {
                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        IgnoreNullValues = true,
                        PropertyNameCaseInsensitive = true,
                        MaxDepth = 2,
                    };

                    jsonSerializerOptions.Converters.Add(new HttpService.Serializer.RequestModelJsonConverter());

                    model = JsonSerializer.Deserialize<RequestModel>(data, jsonSerializerOptions);
                }
            }

            if (context.Request.Method.ToLower() == "get")
            {
                throw new NotImplementedException();
            }

            return model;
        }

        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly HttpContext context;
    }
}
