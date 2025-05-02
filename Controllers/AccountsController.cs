using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmmettPierson.com.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace EmmettPierson.com.Controllers
{
    public class AccountsController : Controller
    {
        private readonly LedgerContext _context;

        public AccountsController(LedgerContext context)
        {
            _context = context;
        }

        // GET: Balance
        public async Task<IActionResult> Bal(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FirstOrDefaultAsync(m => m.Id == id);

            if (account == null)
            {
                return NotFound();
            }

            List<Transaction> transactions = _context.Transactions.Where(t => t.AccountId == id).ToList();

            transactions.Sort((t1, t2) => -t1.TransactionDate.CompareTo(t2.TransactionDate));

            Balance bal = new Balance(account, id, transactions);

            return View(bal);
        }

        // GET: Accounts
        public async Task<IActionResult> Index()
        {
            return View(await _context.Account.ToListAsync());
        }

        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Accounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string firstName, string lastName, string email, string salt, double positiveInterest, double negativeInterest)
        {
            Account account = new Account();
            if (ModelState.IsValid)
            {
                account = new Account(firstName, lastName, email, salt, positiveInterest, negativeInterest); // salt means password, actual salt is random
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string firstName, string lastName, string email, string salt, double positiveInterest, double negativeInterest)
        {
            if (ModelState.IsValid)
            {
                Account? account = _context.Account.Find(id);

                if (account == null)
                {
                    return NotFound();
                }

                try
                {
                    account.FirstName = firstName;
                    account.LastName = lastName;
                    account.Email = email;
                    account.PositiveInterest = positiveInterest;
                    account.NegativeInterest = negativeInterest;
                    account.ChangePassword(salt); // Salt means password right now

                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id))
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
        
            return NotFound();
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Account.FindAsync(id);
            if (account != null)
            {
                _context.Account.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Account.Any(e => e.Id == id);
        }
    }
}
