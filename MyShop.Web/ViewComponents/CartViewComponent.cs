using Microsoft.AspNetCore.Mvc;
using MyShop.Utilities;
using MyShop.Web.Services;

namespace MyShop.Web.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly ISessionCartService _cartService;

        public CartViewComponent(ISessionCartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var count = _cartService.GetCartCount();
            HttpContext.Session.SetInt32(SD.SessionKey, count);
            return View(count);
        }
    }
}
