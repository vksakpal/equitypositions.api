using System.Threading.Channels;
using System.Transactions;

namespace EquityPositions.Api.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int TradeID { get; set; }
        public int Version { get; set; }
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public string ActionType { get; set; } // e.g., INSERT, UPDATE, CANCEL
        public string OrderType { get; set; }  // e.g., Buy, Sell

        internal int OrderTypeId { get; set; }
    }
}
