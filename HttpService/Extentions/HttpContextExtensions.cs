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

            if (model is FileResponseModel)
            {
                var fileResponseModel = (FileResponseModel)model;
                // 파일 응답
                context.Response.Headers[""] = fileResponseModel.FileName;
                if (context.Response.Headers.ContainsKey("Content-Disposition"))
                {
                    context.Response.Headers.Remove("Content-Disposition");
                    //_httpContext.Response.AddHeader("Content-Disposition", "attachment;filename=" + _httpContext.Server.UrlPathEncode(fileInfo.Name));
                    //_httpContext.Response.ContentType = "multipart/form-data";
                }
                context.Response.Headers.Add("Content-Disposition", $"attachment;filename={Uri.EscapeUriString(fileResponseModel.FileName)}");

                //if (context.Response.Headers.ContainsKey("Content-Type"))
                //{
                //    context.Response.Headers.Remove("Content-Type");
                //}
                //context.Response.Headers.Add("Content-Type", fileResponseModel.ContentType);
                context.Response.ContentType = fileResponseModel.ContentType;
                context.Response.BodyWriter.WriteAsync(fileResponseModel.Content);

            }
            else
            {
                // 데이터 응답

                if ("text/xml" == contentType)
                {
                    serializer = new XmlSerializer();
                }

                if ("application/json" == contentType)
                {
                    serializer = new JsonSerializer();
                }

                if (serializer != null)
                {
                    var content = serializer.Serialize(model);

                    context.Response.Headers["content-type"] = contentType;
                    return context.Response.WriteAsync(content);
                }
            }

            return Task.CompletedTask;
        }
    }
}
