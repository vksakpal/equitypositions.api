using EquityPositions.Api.Dtos;
using EquityPositions.Api.Models;
using EquityPositions.Api.Repositories.Abstract;
using EquityPositions.Api.Services.Abstract;
using System.Data;
using System.Threading.Tasks;

namespace EquityPositions.Api.Services.Concrete
{
    public class EquityPositionService : IEquityPositionService
    {
        private readonly ILogger<EquityPositionService> _logger;
        private readonly IPositionCalculator _positionCalculator;
        private readonly IRepository _repository;

        public EquityPositionService(ILogger<EquityPositionService> logger, IPositionCalculator positionCalculator, IRepository repository)
        {
            _logger = logger;
            _positionCalculator = positionCalculator;
            _repository = repository;
        }

        public async Task<bool> ExecuteTransaction(TransactionRequest transactionRequest)
        {
            Transaction transaction = new Transaction();
            transaction.TradeID = transactionRequest.TradeID;
            transaction.Version = GetVersion(transactionRequest.ActionType);
            transaction.Symbol = transactionRequest.Symbol;
            transaction.Quantity = transactionRequest.Quantity;
            transaction.ActionType = GetActionType(transactionRequest.ActionType);
            transaction.OrderTypeId = GetOrderTypeId(transactionRequest.OrderType);

            return await _repository.InsertTransactionAsync(transaction);
        }

        public async Task<EquityPositions.Api.Dtos.EquityPositions> GetEquityPositions()
        {
            DataSet dataset = await _repository.GetAllTransactionsAsync();

            if (dataset.Tables.Count == 0 || dataset.Tables[0].Rows.Count == 0)
            {
                _logger.LogWarning("No transactions found in the database.");
                return new EquityPositions.Api.Dtos.EquityPositions { Positions = new List<EquityPosition>() };
            }

            List<Transaction> transactions = new List<Transaction>();


            foreach (DataRow row in dataset.Tables[0].Rows)
            {
                _logger.LogInformation("Transaction Row: {Row}", string.Join(", ", row.ItemArray));
                Transaction transaction = new Transaction
                {
                    TradeID = Convert.ToInt32(row["ID_TRADE"]),
                    Version = Convert.ToInt32(row["NUM_VERSION"]),
                    Symbol = row["CDE_SYMBOL"].ToString() ?? string.Empty,
                    Quantity = Convert.ToInt32(row["NUM_QTY"]),
                    ActionType = row["TYP_ACTION"].ToString() ?? string.Empty,
                    OrderType = row["TXT_ORDER_TYPE"].ToString() ?? string.Empty
                };
                transactions.Add(transaction);

            }

            Dtos.EquityPositions equityPositions = _positionCalculator.CalculatePositions(transactions);
            return equityPositions;
        }

        private int GetVersion(string actionType)
        {
            if (actionType.Equals("Insert", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        private string GetActionType(string actionType)
        {
            if (actionType.Equals("Insert", StringComparison.OrdinalIgnoreCase))
            {
                return "I";
            }
            else if(actionType.Equals("Update", StringComparison.OrdinalIgnoreCase))
            {
                return "U";            
            }
            else
            {
                return "C";
            }
        }

        private int GetOrderTypeId(string orderType)
        {
            if (orderType.Equals("Buy", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }
            else if (orderType.Equals("Sell", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }
            else
            {
                return -1;
            }
        }
    }
}
