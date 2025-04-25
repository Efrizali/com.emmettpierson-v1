using System.ComponentModel.DataAnnotations;

namespace EmmettPierson.com.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public Account Account { get; set; }

        public int AccountId { get; set; }
        public DateTime TransactionDate { get; set; }

        public bool IsNewBalance { get; set; }

        public double Amount { get; set; }

        [MaxLength(300)]
        public string Descrition { get; set; }

        [MaxLength(30)]
        public string Category { get; set; }

        public Transaction()
        {

        }

        public Transaction(int accountId, bool isNewBalance, double amount, string descrition, string category, DateTime transactionDate)
        {
            Id = 0;
            AccountId = accountId;
            IsNewBalance = isNewBalance;
            Amount = amount;
            Descrition = descrition;
            Category = category;
            TransactionDate = transactionDate;
        }
    }
}
