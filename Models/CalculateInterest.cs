namespace EmmettPierson.com.Models
{
    public class CalculateInterest
    {
        public Account Account { get; set; }
        public int AccountId { get; set; }
        public List<Transaction> Transactions { get; set; }
        
    }
}
