using HttpService.Lib;
using HttpService.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public interface IHttpContextManager
    {
        string GetSessionId();

        string GetRemoteIpAddress();

        bool Check(RequestModel model);

        bool CheckSessionId(string sessionIdFromClient);

        IFormFileCollection GetFormFiles();

        Task<RequestModel> ParseRequestData();
    }
}
