using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-PH");

            //last 7 days
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();


            //Total Income
            int TotalIncome = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = String.Format(culture, "{0:C0}", TotalIncome);

            //Total Expenses
            int TotalExpense = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = String.Format(culture, "{0:C0}", TotalExpense);

            //Balance Amount
            int Balance = TotalIncome - TotalExpense;
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = String.Format(culture,"{0:C0}", Balance);


            //Donut Chart - Expense by Category
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(x => x.Category.CategoryId)
                .Select(j => new
                {
                    categoryTitleWithIcon = j.First().Category.Icon + " " + j.First().Category.Title,
                    amount = j.Sum(x => x.Amount),
                    formattedAmount = j.Sum(x => x.Amount).ToString("C0"),
                })
                .ToList();

            return View();
        }
    }
}
