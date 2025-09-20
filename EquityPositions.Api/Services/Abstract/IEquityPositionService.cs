using EquityPositions.Api.Dtos;

namespace EquityPositions.Api.Services.Abstract
{
    public interface IEquityPositionService
    {
        public Task<Dtos.EquityPositions> GetEquityPositions();
        public Task<bool> ExecuteTransaction(TransactionRequest transactionRequest);
    }
}
