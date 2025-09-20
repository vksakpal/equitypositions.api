using EquityPositions.Api.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Reflection.PortableExecutable;

namespace EquityPositions.Api.Repositories
{
    public class TransactionRepository(IConfiguration configuration, ILogger<TransactionRepository> logger) : IRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection");
        private readonly ILogger<TransactionRepository> _logger = logger;

        public async Task<DataSet> GetAllTransactionsAsync()
        {
            var dataSet = new DataSet();
            using var connection = new SqlConnection(_connectionString);
            try
            {

                await connection.OpenAsync();
                using DbCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "usp_GetTransactions";

                using DbDataReader dbDataReader = await command.ExecuteReaderAsync();
                if (dbDataReader != null)
                {
                    // Create a single table and load all data into it
                    var table = new DataTable("Transactions");
                    table.Load(dbDataReader);
                    dataSet.Tables.Add(table);
                }


            }
            catch (Exception ex)
            {                
                _logger.LogError($"Error opening database connection: {ex.Message}");
                throw; 
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }


            return dataSet;
        }

        public async Task<bool> InsertTransactionAsync(Models.Transaction transaction)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                await connection.OpenAsync();
                using DbCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "usp_InsertTransaction";


                command.Parameters.Add(new SqlParameter("@ID_TRADE", SqlDbType.Int) { Value = transaction.TradeID });
                command.Parameters.Add(new SqlParameter("@NUM_VERSION", SqlDbType.Int) { Value = transaction.Version });
                command.Parameters.Add(new SqlParameter("@CDE_SYMBOL", SqlDbType.VarChar,50) { Value = transaction.Symbol });
                command.Parameters.Add(new SqlParameter("@NUM_QTY", SqlDbType.BigInt) { Value = transaction.Quantity });
                command.Parameters.Add(new SqlParameter("@TYP_ACTION", SqlDbType.Char,1) { Value = transaction.ActionType });
                command.Parameters.Add(new SqlParameter("@ID_ORDER_TYPE", SqlDbType.Int) { Value = transaction.OrderTypeId });

                int result = await command.ExecuteNonQueryAsync();
                if (result >= 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {                
                _logger.LogError($"Error opening database connection: {ex.Message}");
                throw;
            }

        }
    }
}
