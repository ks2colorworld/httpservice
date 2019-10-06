using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public interface IQueryManager: IDisposable
    {
        Task<DataSet> ExecuteQueryAsync(string commandText, Dictionary<string, object> parameters);
    }
}
