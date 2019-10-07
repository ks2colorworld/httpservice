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
    public class MySqlQueryManager : IQueryManager
    {
        public MySqlQueryManager(
            IOptionsMonitor<AppOptions> appOptionAccessor,
            ILoggerFactory loggerFactory)
        {
            appOptions = appOptionAccessor.CurrentValue;
            logger = loggerFactory.CreateLogger<MySqlQueryManager>();

            connection = new MySqlConnection(appOptions.DbConnectionConfig);
        }

        public void Dispose()
        {
            logger.LogInformation($"{nameof(MySqlQueryManager)}.{nameof(Dispose)} : Dispose connection.");
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
            logger.LogInformation($"{nameof(MySqlQueryManager)}.{nameof(ExecuteQueryAsync)} : Started");

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
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        logger.LogInformation($"{nameof(MySqlQueryManager)}.{nameof(ExecuteQueryAsync)} : Fill DataSet");
                        await adapter.FillAsync(dataset);

                        await transaction.CommitAsync();
                        logger.LogInformation($"{nameof(MySqlQueryManager)}.{nameof(ExecuteQueryAsync)} : Commit transaction.");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    logger.LogInformation($"{nameof(MySqlQueryManager)}.{nameof(ExecuteQueryAsync)} : Rollback transaction.");

                    logger.LogError(ex, $"{nameof(MySqlQueryManager)}.{nameof(ExecuteQueryAsync)} : Exception");

                    throw new ServiceException("ReturnDataSet_Common.Error check(ExecuteDataset)", ex);
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return dataset;
        }

        private MySqlParameter CreateParameter(string name, object value)
        {
            return new MySqlParameter(name, value);
        }

        private readonly AppOptions appOptions;
        private readonly MySqlConnection connection;
        private readonly ILogger logger;
    }
}
