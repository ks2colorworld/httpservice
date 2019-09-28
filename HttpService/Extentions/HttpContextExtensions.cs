using HttpService.Models;
using HttpService.Serializer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HttpService
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// <see cref="ResponseModel"/> 객체로 응답을 작성합니다.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Task ExecuteResponseModelResult(this HttpContext context, ResponseModel model)
        {
            ISerializer serializer = null;
            var contentType = context.Request.Headers["Accept"];
            if ("text/xml" == contentType)
            {
                serializer = new XmlSerializer();
            }
            
            if("application/json" == contentType)
            {
                serializer = new JsonSerializer();
            }

            if (serializer != null)
            {
                var content = serializer.Serialize(model);

                context.Response.Headers["content-type"] = contentType;
                return context.Response.WriteAsync(content);
            }

            return Task.CompletedTask;
        }
    }
}
