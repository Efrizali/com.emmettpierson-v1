﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmmettPierson.com.Models;
using Microsoft.Identity.Client;
using Humanizer;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EmmettPierson.com.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly LedgerContext _context;

        public TransactionsController(LedgerContext context)
        {
            _context = context;

            // Spin off a thread to handle daily interest
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            // Use this to delete interest transactions because they get annoying
            //_context.Transactions.RemoveRange(_context.Transactions.Where(t => t.Category == "Interest"));
            //_context.SaveChanges();



            var ledgerContext = _context.Transactions.Include(t => t.Account);
            return View(await ledgerContext.ToListAsync());
        }

        // Callable way to test interest calculations, would be an async operation in a normal service that always runs, but that isn't this project
        // Change the input to change how much interest is applied. 365 is daily interest, 12 is monthly
        // GET: Transactions/CalculateInterest
        public async Task<IActionResult> CalculateInterest()
        {
            CreateInterestTransactions();

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

        // Creating a way to calculate interest when required
        public async void CreateInterestTransactions()
        {
            List<Account> accounts = await _context.Account.ToListAsync();

            foreach (Account account in accounts)
            {
                List<Transaction> transactions = _context.Transactions.Where(t => t.AccountId == account.Id).ToList();

                transactions.Sort((t1, t2) => t1.TransactionDate.CompareTo(t2.TransactionDate));

                // Get the newest New Balance
                Transaction newestBalance = null;
                foreach (Transaction transaction in transactions)
                {
                    if (!transaction.IsNewBalance) continue;

                    if (newestBalance == null)
                    {
                        newestBalance = transaction;
                        continue;
                    }

                    if (newestBalance.TransactionDate.CompareTo(transaction.TransactionDate) > 0)
                    {
                        newestBalance = transaction;
                    }
                }

                // For debug purposes, if there is already an interest bearing transaction today, do another one anyway and don't do anything else.
                // Needs to have access to newestBalance
                // The rest of this method works great, but this portion would be deleted in production
                bool interestToday = false;
                List<Transaction> todaysTransactions = transactions.Where(t => t.TransactionDate.AtNoon().CompareTo(DateTime.Now.AtNoon()) == 0).ToList();
                foreach (Transaction transaction in todaysTransactions)
                {
                    if (transaction.Category == "Interest")
                    {
                        // Adjust time to still add interest even if the balance is after noon by adding the interest to tomorrow
                        if (transaction.TransactionDate.CompareTo(newestBalance.TransactionDate) > 0)
                        {
                            Balance balance = new Balance(account, transactions);

                            _context.Transactions.Add(Transaction.GetInterestTransaction(account, balance.Total, DateTime.Now.AddDays(1).AtNoon()));
                        }
                        else
                        {
                            Balance balance = new Balance(account, transactions);

                            _context.Transactions.Add(Transaction.GetInterestTransaction(account, balance.Total, DateTime.Now.AtNoon()));
                        }
                        interestToday = true;

                        break;
                    }
                }
                if (interestToday) continue;

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

                            interestTransactions.Add(Transaction.GetInterestTransaction(account, principal, startingDate.AtNoon()));

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

                            interestTransactions.Add(Transaction.GetInterestTransaction(account, principal, startingDate.AtNoon()));

                            principal += interestTransactions.Last().Amount;

                            startingDate = startingDate.AddDays(1);
                        }
                    }
                }

                _context.Transactions.AddRange(interestTransactions);
            }
            _context.SaveChanges();
        }
        

        // Developed too late to test properly
        public async void InterestHandler()
        {
            // Ensure the program is fully up and running before starting the interest calculations
            await Task.Delay(TimeSpan.FromSeconds(120));

            // This will add interest every time it is run while that development for loop exists, but without it it just ensures that interest is up to date.
            CreateInterestTransactions();
            bool interestCalculatedToday = true;

            while (true)
            {
                DateTime currentTime = DateTime.Now;

                // If it is after midnight and before noon
                if (currentTime.CompareTo(currentTime.AtMidnight()) >= 0 
                    && currentTime.CompareTo(currentTime.AtNoon()) <= 0)
                {
                    interestCalculatedToday = false;
                } 
                // If it is after noon today and before midnight tomorrow
                else if (currentTime.CompareTo(currentTime.AtNoon()) >= 0
                    && currentTime.CompareTo(currentTime.AddDays(1).AtMidnight()) <= 0)
                {
                    if (!interestCalculatedToday)
                    {
                        CreateInterestTransactions();
                        interestCalculatedToday = true;
                    }
                }

                TimeSpan delay = TimeSpan.Zero;

                if (interestCalculatedToday)
                {
                    double timeBetween = currentTime.AddDays(1).AtMidnight().Subtract(currentTime).TotalMinutes;
                    timeBetween += 2; // Add 2 minutes so that it always happens a few minutes after the intended time instead of before
                    timeBetween /= 2; // I want it to run twice each period to ensure no interest bearing period is missed
                    delay = TimeSpan.FromMinutes(timeBetween);
                }
                else
                {
                    double timeBetween = currentTime.AtNoon().Subtract(currentTime).TotalMinutes;
                    timeBetween += 2; // Add 2 minutes so that it always happens a few minutes after the intended time instead of before
                    timeBetween /= 2; // I want it to run twice each period to ensure no interest bearing period is missed
                    delay += TimeSpan.FromMinutes(timeBetween);
                }

                await Task.Delay(delay);
            }
        }
    }
}
