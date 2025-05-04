namespace EmmettPierson.com.Models
{
    public class Balance
    {
        public Account Account { get; set; }
        public int AccountId { get; set; }
        public List<Transaction> Transactions { get; set; }
        public double Total { get; set; }

        public Balance()
        {

        }
        public Balance(Account account, List<Transaction> transactions)
        {

            Account = account;
            AccountId = account.Id;
            Transactions = transactions;
            Total = 0;


            // Calculates the total
            if (transactions == null)
            {
                return;
            }

            List<Transaction> newBalances = transactions.Where(t => t.IsNewBalance).ToList();

            newBalances.Sort((t1, t2) => -t1.TransactionDate.CompareTo(t2.TransactionDate));

            Transaction newest = newBalances.First();

            List<Transaction> relaventTransactions = transactions.Where(t => t.TransactionDate.CompareTo(newest.TransactionDate) > 0).ToList();

            Total = newest.Amount;

            foreach (Transaction t in relaventTransactions)
            {
                Total += t.Amount;
            }
        }
    }
}
