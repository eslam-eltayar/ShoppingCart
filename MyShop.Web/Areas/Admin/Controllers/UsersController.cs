using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.DataAccess.Data;
using MyShop.Utilities;
using System.Security.Claims;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;

            var users = _context.AppUsers.Where(X => X.Id != userId).ToList();

            return View(users);
        }


        public IActionResult LockUnlock(string? id)
        {
            var user = _context.AppUsers.FirstOrDefault(U => U.Id == id);

            if (user is null)
                return NotFound();

            if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
                user.LockoutEnd = DateTime.Now.AddMonths(3); // will lock account for 3 months
            else
                user.LockoutEnd = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Index", "Users", new { area = SD.AdminRole });

        }
    }
}
