using HttpService.Lib;
using HttpService.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public interface IDatabaseManager : IDisposable
    {
        /// <summary>
        /// 저장 프로시저 이름을 가져옵니다.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        string GetProcedureName(RequestModel model);

        /// <summary>
        /// 저장 프로시저를 실행합니다.
        /// </summary>
        /// <param name="model">HTTP 요청 데이터</param>
        /// <param name="includeSeesionId">세션식별자 포함 여부; <see cref="AppendSessionId"/></param>
        /// <returns></returns>
        Task<DataSet> ExecuteQueryAsync(RequestModel model, bool includeSeesionId = false);

        Task<DataSet> ExecuteQueryAsync(string commandText, Dictionary<string, object> parameters);

        //[Obsolete]
        //Task<DataSet> ExecuteQueryAsync(string commandText, IEnumerable<MySqlParameter> parameters);

        //Task<DataSet> Attachment_CRDAsync(CRUD crd, RequestModel requestModel, FileModel fileModel);
    }
}
