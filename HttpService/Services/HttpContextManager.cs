using HttpService.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public class HttpContextManager : IHttpContextManager
    {
        public HttpContextManager(
            IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<RequestModel> ParseRequestData()
        {
            RequestModel model = null;
            if (httpContext.Request.Method.ToLower() == "post")
            {
                var contentType = String.Empty;
                if (httpContext.Request.Headers.ContainsKey("Content-Type"))
                {
                    contentType = httpContext.Request.Headers["Content-Type"].ToString();
                }

                var data = String.Empty;

                if (contentType.ToLower().Contains("multipart/form-data"))
                {
                    model = new RequestModel();
                    var formData = httpContext.Request.Form;
                    foreach (var item in formData)
                    {
                        model.Add(item.Key, item.Value);
                    }
                }

                if (contentType.ToLower() == "application/json")
                {
                    using (var reader = new StreamReader(httpContext.Request.Body))
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
            }

            if (httpContext.Request.Method.ToLower() == "get")
            {
                throw new NotImplementedException();
            }

            return model;
        }

        /// <summary>
        /// 세션 식별자를 GUID 형식의 문자열로 가져옵니다.
        /// </summary>
        /// <returns>세션 식별자 (GUID)</returns>
        public string GetSessionId()
        {
            return httpContext.Session.Id;
        }

        public string GetRemoteIpAddress()
        {
            return httpContext.Connection.RemoteIpAddress.ToString();
        }

        /// <summary>
        /// 현재 세션 식별자와 클라이언트에서 요청한 세션 식별자를 확인합니다.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Check(RequestModel model)
        {
            bool isSameSessionID = true;

            bool isWebQuery = model.HasKey(Constants.WEB_GUBUN_KEY);
            string client_SessionID = model.GetValue(Constants.SESSIONID_KEY_STRING);
            string proc = model.GetValue(Constants.PROC_KEY_STRING);
            string gubun = model.GetValue(Constants.GUBUN_KEY_STRING);

            //bool exclude = isWebQuery || gubun.Equals(Constants.USER_LOGIN_GUBUN, StringComparison.OrdinalIgnoreCase) || gubun.Equals(Constants.GET_SESSIONID_GUBUN, StringComparison.OrdinalIgnoreCase);

            if (!IsExcludeCondition(isWebQuery, gubun))
            {
                isSameSessionID = EqualsSession(GetSessionId(), client_SessionID);
            }

            return isSameSessionID;
        }

        public bool CheckSessionId(string sessionIdFromClient)
        {
            var session = GetSessionId();

            return EqualsSession(session, sessionIdFromClient);
        }

        public IFormFileCollection GetFormFiles()
        {
            return httpContext.Request.Form.Files;
        }

        /// <summary>
        /// 세션 식별자 확인 예외사항
        /// </summary>
        /// <param name="isWebQuery"></param>
        /// <param name="gubun"></param>
        /// <returns></returns>
        private bool IsExcludeCondition(bool isWebQuery, string gubun)
        {
            if (isWebQuery) { return true; }

            if (gubun.Equals(Constants.USER_LOGIN_GUBUN, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if(gubun.Equals(Constants.GET_SESSIONID_GUBUN, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 식별자가 동일한지 확인합니다.
        /// </summary>
        /// <param name="serverSessionId"></param>
        /// <param name="clientSessionId"></param>
        /// <returns></returns>
        private bool EqualsSession(string serverSessionId, string clientSessionId)
        {
            if (String.IsNullOrEmpty(serverSessionId)) { return false; }
            if (String.IsNullOrEmpty(clientSessionId)) { return false; }

            return serverSessionId.Equals(clientSessionId, StringComparison.OrdinalIgnoreCase);
        }

        private readonly HttpContext httpContext;
    }
}
