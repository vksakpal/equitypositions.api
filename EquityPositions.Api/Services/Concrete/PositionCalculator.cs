using EquityPositions.Api.Dtos;
using EquityPositions.Api.Models;
using EquityPositions.Api.Services.Abstract;

namespace EquityPositions.Api.Services.Concrete
{
    public class PositionCalculator : IPositionCalculator
    {
        public Dtos.EquityPositions CalculatePositions(List<Transaction> transactions)
        {         

            var positions = new Dictionary<string, int>();

            foreach (var tx in transactions)
            {
                positions.TryAdd(tx.Symbol, 0);
   
                if (tx.ActionType.Equals("C", StringComparison.OrdinalIgnoreCase))
                {
                    positions[tx.Symbol] = 0;
                    continue;
                }

                
                if (tx.ActionType.Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    if(tx.OrderType.Equals("Buy", StringComparison.OrdinalIgnoreCase))
                    {
                        
                        
                        positions[tx.Symbol] = positions[tx.Symbol] + tx.Quantity; 
                    }
                    else if(tx.OrderType.Equals("Sell", StringComparison.OrdinalIgnoreCase))
                    {
                  
                        positions[tx.Symbol] = positions[tx.Symbol] - tx.Quantity;
                    }
                }
                
                if(tx.ActionType.Equals("U", StringComparison.OrdinalIgnoreCase))
                {
                    if (tx.OrderType.Equals("Buy", StringComparison.OrdinalIgnoreCase))
                    {

                        
                        positions[tx.Symbol] = tx.Quantity;
                    }
                    else if (tx.OrderType.Equals("Sell", StringComparison.OrdinalIgnoreCase))
                    {
                   
                        positions[tx.Symbol] = tx.Quantity;
                    }
                }
                else
                {
                    continue; 
                }

                
            }

           



            Dtos.EquityPositions equityPositions = new Dtos.EquityPositions();

            equityPositions.Positions = positions?.Select(static ep => new EquityPosition { Symbol = ep.Key, Quantity = ep.Value })?.ToList() ?? Enumerable.Empty<EquityPosition>().ToList();

            return equityPositions;
        }
    }
}
