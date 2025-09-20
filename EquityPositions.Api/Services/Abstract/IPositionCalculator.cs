using EquityPositions.Api.Models;

namespace EquityPositions.Api.Services.Abstract
{
    public interface IPositionCalculator
    {
        public Dtos.EquityPositions CalculatePositions(List<Transaction> transactions);
    
    }
}
