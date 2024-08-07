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
    }
}
