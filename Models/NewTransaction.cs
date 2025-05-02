namespace EmmettPierson.com.Models
{
    public class NewTransaction
    {
        public Account Account { get; set; }
        public int AccountId { get; set; }
        public Transaction Transaction { get; set; }
        public int TransactionId { get; set; }

        public NewTransaction(int accountId)
        {
            AccountId = accountId;
        }

        public NewTransaction()
        {
        }
    }
}
