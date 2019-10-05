using HttpService.Lib;
using HttpService.Models;
using HttpService.Options;
using Microsoft.AspNetCore.Http;
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
    public interface IDatabaseManager : IDisposable
    {
        /// <summary>
        /// 저장 프로시저를 실행합니다.
        /// </summary>
        /// <param name="model">HTTP 요청 데이터</param>
        /// <param name="includeSeesionId">세션식별자 포함 여부; <see cref="AppendSessionId"/></param>
        /// <returns></returns>
        Task<DataSet> ExecuteQueryAsync(RequestModel model, bool includeSeesionId = false);


        Task<DataSet> ExecuteQueryAsync(string commandText, IEnumerable<MySqlParameter> parameters);


        Task<DataSet> Attachment_CRDAsync(CRUD crd, RequestModel requestModel, FileModel fileModel);
    }

    public class DatabaseManager : IDatabaseManager
    {
        public DatabaseManager(
            IHttpContextManager httpContextManager,
            IOptions<AppOptions> appOptionAccessor,
            ILoggerFactory loggerFactory)
        {
            this.httpContextManager = httpContextManager;
            appOptions = appOptionAccessor.Value;
            connection = new MySqlConnection(appOptions.DbConnectionConfig);
            logger = loggerFactory.CreateLogger<DatabaseManager>();
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

            string storeProcedureName = model.GetValue(XMLCommonUtil.PROC_KEY_STRING);
            string schema = appOptions.DbSchema;
            if (!String.IsNullOrEmpty(schema))
            {
                storeProcedureName = $"{schema}.{storeProcedureName}";
            }

            try
            {
                dataset = await ExecuteQueryAsync(storeProcedureName, model.Parameters.Select(parameter => new MySqlParameter(parameter.Key, parameter.Value)));
                if (dataset != null && dataset.Tables.Count > 0)
                {
                    dataset.DataSetName = Constants.DATASET_NAME;

                    if (AppendSessionId(includeSeesionId, dataset))
                    {
                        logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Includes session id");
                        dataset.Namespace = httpContextManager.GetSessionId();
                    }

                    dataset.Tables[0].TableName = XMLCommonUtil.TABLE_NAME;
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

        /// <summary>
        /// 저장 프로시저를 실행합니다
        /// </summary>
        /// <param name="commandText">저장 프로시저 이름</param>
        /// <param name="parameters">매개변수</param>
        /// <returns></returns>
        public async Task<DataSet> ExecuteQueryAsync(string commandText, IEnumerable<MySqlParameter> parameters)
        {
            logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Started");

            var dataset = new DataSet();
            try
            {
                await connection.OpenAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException("ReturnDataSet_Common.Error check(ExecuteDataset); connection.Open()", ex);
            }

            using (var transaction = connection.BeginTransaction())
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                if (parameters != null && parameters.Count() > 0)
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                try
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Fill DataSet");
                        await adapter.FillAsync(dataset);

                        await transaction.CommitAsync();
                        logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Commit transaction.");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    logger.LogInformation($"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Rollback transaction.");

                    logger.LogError(ex, $"{nameof(DatabaseManager)}.{nameof(ExecuteQueryAsync)} : Exception");

                    throw new ServiceException("ReturnDataSet_Common.Error check(ExecuteDataset)", ex);
                }
            }

            return dataset;
        }

        /// <summary>
        /// 파일 정보를 입출력합니다.
        /// </summary>
        /// <param name="crd">입출력</param>
        /// <param name="requestModel"></param>
        /// <param name="fileModel"></param>
        /// <returns></returns>
        public async Task<DataSet> Attachment_CRDAsync(CRUD crd, RequestModel requestModel, FileModel fileModel)
        {
            string proc_name = Constants.ATTACHMENT_R_value;
            switch (crd)
            {
                case CRUD.C:
                case CRUD.U:
                case CRUD.D:
                    proc_name = Constants.ATTACHMENT_CUD_value;
                    break;
                case CRUD.R:
                    proc_name = Constants.ATTACHMENT_R_value;
                    break;
            }

            if (!String.IsNullOrEmpty(appOptions.DbSchema))
            {
                proc_name = $"{appOptions.DbSchema}.{proc_name}";
            }

            var parameters = new List<MySqlParameter>();
            parameters.Add(CreateParamter(Constants.GUBUN_KEY_STRING, crd.ToString()));
            parameters.Add(CreateParamter(Constants.OPERATOR_IP_GUBUN, httpContextManager.GetRemoteIpAddress()));
            parameters.Add(CreateParamter(Constants.OPERATOR_KEY_GUBUN, requestModel.GetValue(Constants.OPERATOR_KEY_GUBUN)));

            bool include_organization_key = Constants.INCLUDE_ORGANIZATION_KEY;
            if (include_organization_key)
            {
                parameters.Add(CreateParamter(Constants.ORGANIZATION_KEY_GUBUN, requestModel.GetValue(Constants.ORGANIZATION_KEY_GUBUN)));
            }

            switch (crd)
            {
                case CRUD.R:
                case CRUD.D:
                    parameters.Add(CreateParamter(Constants.ATTACHMENT_KEY_key, fileModel.Attachment_key));
                    break;
                case CRUD.C:
                    parameters.Add(CreateParamter(Constants.ATTACHMENT_GUBUN_key, requestModel.GetValue(Constants.ATTACHMENT_GUBUN_key)));
                    parameters.Add(CreateParamter(Constants.ATTACHMENT_DETAIL_CODE_key, requestModel.GetValue(Constants.ATTACHMENT_DETAIL_CODE_key)));
                    parameters.Add(CreateParamter(Constants.ATTACHMENT_FILENAME_key, fileModel.Name));
                    parameters.Add(CreateParamter(Constants.ATTACHMENT_FILEFORMAT_key, fileModel.Format));
                    parameters.Add(CreateParamter(Constants.ATTACHMENT_FILESIZE_key, fileModel.Size));
                    break;
            }

            DataSet dataSet = await ExecuteQueryAsync(proc_name, parameters);

            return dataSet;
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

        private MySqlParameter CreateParamter(string name, object value)
        {
            return new MySqlParameter(name, value);
        }

        private readonly IHttpContextManager httpContextManager;
        private readonly AppOptions appOptions;
        private readonly MySqlConnection connection;
        private readonly ILogger logger;
    }
}
