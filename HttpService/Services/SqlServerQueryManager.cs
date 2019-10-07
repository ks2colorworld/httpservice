using HttpService.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public class SqlServerQueryManager : IQueryManager
    {
        public const string PARAMETER_PREFIX = "@";

        public SqlServerQueryManager(
         IOptionsMonitor<AppOptions> appOptionAccessor,
         ILoggerFactory loggerFactory)
        {
            appOptions = appOptionAccessor.CurrentValue;
            logger = loggerFactory.CreateLogger<MySqlQueryManager>();

            connection = new SqlConnection(appOptions.DbConnectionConfig);
        }

        public void Dispose()
        {
            logger.LogInformation($"{nameof(SqlServerQueryManager)}.{nameof(Dispose)} : Dispose connection.");
            if (connection != null)
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        public async Task<DataSet> ExecuteQueryAsync(string commandText, Dictionary<string, object> parameters)
        {
            logger.LogInformation($"{nameof(SqlServerQueryManager)}.{nameof(ExecuteQueryAsync)} : Started");

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
                command.Transaction = transaction;

                if (parameters != null && parameters.Count() > 0)
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(CreateParameter(parameter.Key, parameter.Value));
                    }
                }

                try
                {
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        logger.LogInformation($"{nameof(SqlServerQueryManager)}.{nameof(ExecuteQueryAsync)} : Fill DataSet");
                        adapter.Fill(dataset);

                        await transaction.CommitAsync();

                        logger.LogInformation($"{nameof(SqlServerQueryManager)}.{nameof(ExecuteQueryAsync)} : Commit transaction.");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    logger.LogInformation($"{nameof(SqlServerQueryManager)}.{nameof(ExecuteQueryAsync)} : Rollback transaction.");

                    logger.LogError(ex, $"{nameof(SqlServerQueryManager)}.{nameof(ExecuteQueryAsync)} : Exception");

                    throw new ServiceException("ReturnDataSet_Common.Error check(ExecuteDataset)", ex);
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return dataset;
        }

        private SqlParameter CreateParameter(string name, object value)
        {
            return new SqlParameter($"{PARAMETER_PREFIX}{name}", value);
        }

        private readonly AppOptions appOptions;
        private readonly SqlConnection connection;
        private readonly ILogger logger;
    }
}
