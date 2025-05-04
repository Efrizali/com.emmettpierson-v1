using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmmettPierson.com.Models;
using Microsoft.Identity.Client;
using Humanizer;

namespace EmmettPierson.com.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly LedgerContext _context;

        public TransactionsController(LedgerContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var ledgerContext = _context.Transactions.Include(t => t.Account);
            return View(await ledgerContext.ToListAsync());
        }

        // Callable way to test interest calculations, would be an async operation in a normal service that always runs, but that isn't this project
        // Warning, lots of List<Transaction> lists because yes. 
        // GET: Transactions/CalculateInterest
        public async Task<IActionResult> CalculateInterest()
        {
            List<Account> accounts = await _context.Account.ToListAsync();

            foreach (Account account in accounts)
            {
                List<Transaction> transactions = _context.Transactions.Where(t => t.AccountId == account.Id).ToList();

                // For debug purposes, if there is already an interest bearing transaction today, do another one anyway and don't do anything else
                bool interestToday = false;
                List<Transaction> todaysTransactions = transactions.Where(t => t.TransactionDate.AtNoon().CompareTo(DateTime.Now.AtNoon()) == 0).ToList();
                foreach (Transaction transaction in todaysTransactions)
                {
                    if (transaction.Category == "Interest")
                    {
                        Balance balance = new Balance(account, transactions);

                        _context.Transactions.Add(Transaction.GetInterestTransaction(account, balance.Total, DateTime.Now));

                        interestToday = true;

                        break;
                    }
                }
                if (interestToday) continue;


                // Get the newest New Balance
                Transaction newestBalance = null;
                foreach (Transaction transaction in transactions)
                {
                    if (!transaction.IsNewBalance) continue;

                    if (newestBalance == null) {
                        newestBalance = transaction;
                        continue;
                    }

                    if (newestBalance.TransactionDate.CompareTo(transaction.TransactionDate) > 0)
                    {
                        newestBalance = transaction;
                    }
                }

                // Transactions after the newest balance (including the newest balance)
                List<Transaction> relaventTransactions = transactions.Where(t => t.TransactionDate.CompareTo(newestBalance.TransactionDate) >= 0).ToList();

                // Each transaction created below will be added to interestTransactions
                List<Transaction> interestTransactions = new List<Transaction>();
                double principal = 0;

                for (int i = 0; i < relaventTransactions.Count; i++)
                {
                    principal += relaventTransactions[i].Amount;

                    // No interest if it is past noon when the new balance goes up
                    if (i == 0 && relaventTransactions[0].TransactionDate.CompareTo(relaventTransactions[0].TransactionDate.AtNoon()) < 0) continue;

                    // Using this and the foreach in the while loop, interest isn't doubled up each time this is run
                    if (relaventTransactions[i].Category == "Interest") continue;

                    DateTime startingDate = relaventTransactions[i].TransactionDate.AtNoon();
                    if (i == relaventTransactions.Count - 1)
                    {
                        // If this is the last transaction
                        
                        
                        // Create interest transactions until we reach today
                        while (startingDate.CompareTo(DateTime.Now.AtNoon()) <= 0)
                        {

                            // If an interest bearing transaction exists for this day, continue
                            interestToday = false;
                            List<Transaction> transactionsOnStartingDate = relaventTransactions.Where(t => t.TransactionDate.CompareTo(startingDate) == 0).ToList();
                            foreach (Transaction transaction in transactionsOnStartingDate)
                            {
                                if (transaction.Category == "Interest") interestToday = true;
                            }
                            if (interestToday) continue;
                            
                            interestTransactions.Add(Transaction.GetInterestTransaction(account, principal, startingDate));

                            principal += interestTransactions.Last().Amount;

                            startingDate = startingDate.AddDays(1);
                        }
                    } 
                    else
                    {
                        while (startingDate.CompareTo(relaventTransactions[i + 1].TransactionDate.AtNoon()) <= 0)
                        {
                            // If an interest bearing transaction exists for this day, continue
                            interestToday = false;
                            List<Transaction> transactionsOnStartingDate = relaventTransactions.Where(t => t.TransactionDate.CompareTo(startingDate) == 0).ToList();
                            foreach (Transaction transaction in transactionsOnStartingDate)
                            {
                                if (transaction.Category == "Interest") interestToday = true;
                            }
                            if (interestToday) continue;

                            interestTransactions.Add(Transaction.GetInterestTransaction(account, principal, startingDate));

                            principal += interestTransactions.Last().Amount;

                            startingDate = startingDate.AddDays(1);
                        }
                    }
                }

                _context.Transactions.AddRange(interestTransactions);
            }
            _context.SaveChanges();

            return View();
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            ViewData["AccountId"] = new SelectList(_context.Account, "Id", "Id");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int accountId, bool isNewBalance, double amount, string descrition, string category)
        {
            Transaction transaction = new Transaction();
            if (ModelState.IsValid)
            {
                transaction = new Transaction(accountId, isNewBalance, amount, descrition, category, DateTime.Now);
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Account, "Id", "Id", transaction.AccountId);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.Account, "Id", "Id", transaction.AccountId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AccountId,IsNewBalance,Amount,Descrition,Category")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Account, "Id", "Id", transaction.AccountId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
