using HttpService.Models;
using Microsoft.AspNetCore.Http;
using System;

namespace HttpService.Services
{
    public class HttpContextManager : IHttpContextManager
    {
        public HttpContextManager(
            IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
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
            string proc = model.GetValue(Constants. PROC_KEY_STRING);
            string gubun = model.GetValue(Constants.GUBUN_KEY_STRING);

            //bool exclude = isWebQuery || gubun.Equals(Constants.USER_LOGIN_GUBUN, StringComparison.OrdinalIgnoreCase) || gubun.Equals(Constants.GET_SESSIONID_GUBUN, StringComparison.OrdinalIgnoreCase);

            if (!IsExcludeCondition(isWebQuery, gubun))
            {
                isSameSessionID = EqualsSession(client_SessionID, GetSessionId());
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
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool EqualsSession(string x, string y)
        {
            if (String.IsNullOrEmpty(x)) { return false; }
            if (String.IsNullOrEmpty(y)) { return false; }

            return x.Equals(y, StringComparison.OrdinalIgnoreCase);
        }

        private readonly HttpContext httpContext;
    }
}
