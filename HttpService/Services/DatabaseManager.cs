using HttpService.Lib;
using HttpService.Models;
using HttpService.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public class DatabaseManager : IDatabaseManager
    {
        public DatabaseManager(
            IHttpContextManager httpContextManager,
            IQueryManager queryManager,
            IOptions<AppOptions> appOptionAccessor,
            ILoggerFactory loggerFactory)
        {
            this.httpContextManager = httpContextManager;
            this.queryManager = queryManager;
            appOptions = appOptionAccessor.Value;
            connection = new MySqlConnection(appOptions.DbConnectionConfig);
            logger = loggerFactory.CreateLogger<DatabaseManager>();
        }

        public string GetProcedureName(RequestModel model)
        {
            string proc_gubun = model.GetValue(Constants.PROC_KEY_STRING);
            string web_gubun = model.GetValue(Constants.WEB_GUBUN_KEY);

            bool isWebQuery = false;
            bool isAllowProc = false;

            if (!String.IsNullOrEmpty(web_gubun))
            {
                isWebQuery = true;

                //*
                foreach (string str in appOptions.AllowProcedureList)
                {
                    if (proc_gubun.Equals(str, StringComparison.OrdinalIgnoreCase))
                    {
                        isAllowProc = true;
                        break;
                    }
                }//*/
            }

            if ((isWebQuery && !isAllowProc) || String.IsNullOrEmpty(proc_gubun))
            {
                throw new ServiceException("실행이 허가되지 않았거나, 유효하지 않은 프로시져명입니다.");
            }
            else
            {
                if (!String.IsNullOrEmpty(appOptions.DbSchema))
                {
                    proc_gubun = $"{appOptions.DbSchema}.{proc_gubun}";
                }

                return proc_gubun;
            }
        }

        /// <summary>
        /// 저장 프로시저를 실행합니다.
        /// </summary>
        /// <param name="model">HTTP 요청 데이터</param>
        /// <param name="includeSeesionId">세션식별자 포함 여부; <see cref="AppendSessionId"/></param>
        /// <returns></returns>
        public async Task<DataSet> ExecuteQueryAsync(RequestModel model, bool includeSeesionId = false)
        {
            logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Started");

            var dataset = new DataSet();

            //string storeProcedureName = model.GetValue(XMLCommonUtil.PROC_KEY_STRING);
            //string schema = appOptions.DbSchema;
            //if (!String.IsNullOrEmpty(schema))
            //{
            //    storeProcedureName = $"{schema}.{storeProcedureName}";
            //}

            string storeProcedureName = GetProcedureName(model);

            try
            {
                dataset = await ExecuteQueryAsync(storeProcedureName, model.Data.ToDictionary(item => item.Key, item => (object)item.Value));

                if (dataset != null && dataset.Tables.Count > 0)
                {
                    dataset.DataSetName = Constants.DATASET_NAME;

                    if (AppendSessionId(includeSeesionId, dataset))
                    {
                        logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Includes session id");
                        dataset.Namespace = httpContextManager.GetSessionId();
                    }

                    dataset.Tables[0].TableName = Constants.TABLE_NAME;
                }

                logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Commit transaction.");
            }
            catch (Exception ex)
            {
                logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Rollback transaction.");

                logger.LogError(ex, $"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Exception");

                throw new ServiceException("ReturnDataSet_Common.Error check(ExecuteDataset)", ex);
            }
            
            return dataset;
        }

        public Task<DataSet> ExecuteQueryAsync(string commandText, Dictionary<string, object> parameters)
        {
            return queryManager.ExecuteQueryAsync(commandText, parameters);
        }

        public void Dispose()
        {
            logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(Dispose)} : Dispose connection.");
            if (connection != null)
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 세션 식별자 포함여부를 결정합니다.
        /// </summary>
        /// <param name="includeSessionId"></param>
        /// <param name="dataset"></param>
        /// <returns></returns>
        private bool AppendSessionId(bool includeSessionId, DataSet dataset)
        {
            if (!includeSessionId) { return false; }

            if(dataset == null) { return false; }

            if(dataset.Tables.Count == 0)
            {
                return false;
            }

            if (!dataset.Tables[0].Columns.Contains("return_code"))
            {
                return false;
            }
            var rowsCount = dataset.Tables[0].Select("return_code = 1").Length;
            
            if (rowsCount == 0) { return false; }

            return true;
        }

        private readonly IHttpContextManager httpContextManager;
        private readonly IQueryManager queryManager;
        private readonly AppOptions appOptions;
        private readonly MySqlConnection connection;
        private readonly ILogger logger;
    }
}
