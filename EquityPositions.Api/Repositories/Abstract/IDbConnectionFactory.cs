using Microsoft.Data;
using System.Data;

namespace EquityPositions.Api.Repositories.Abstract
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync();
    }
}
