using System.Data;

namespace EquityPositions.Api.Repositories.Abstract
{
    public interface IRepository
    {
        public Task<bool> InsertTransactionAsync(Models.Transaction transaction);

        public Task<DataSet> GetAllTransactionsAsync();
    }
}
