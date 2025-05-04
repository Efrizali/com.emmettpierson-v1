namespace EmmettPierson.com.Models
{
    public class NewTransactions
    {
        public Account Account { get; set; }
        public int AccountId { get; set; }
        public List<Transaction> Transactions { get; } = new List<Transaction>();

        public NewTransactions(Account account, List<Transaction> transactions)
        {
            Account = account;
            AccountId = account.Id;
            Transactions.AddRange(transactions);
        }

        public NewTransactions()
        {
        }
    }
}
