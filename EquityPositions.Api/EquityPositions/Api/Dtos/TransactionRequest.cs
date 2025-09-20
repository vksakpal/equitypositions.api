namespace EquityPositions.Api.Dtos
{
    public class TransactionRequest
    {        
        public int TradeID { get; set; }
        
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public string ActionType { get; set; } // e.g., INSERT, UPDATE, CANCEL
        public string OrderType { get; set; }  // e.g., Buy, Sell
     
    }
}