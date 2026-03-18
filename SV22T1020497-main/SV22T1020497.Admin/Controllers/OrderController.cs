using Microsoft.AspNetCore.Mvc;
using SV22T1020497.Models.Sales;

namespace SV22T1020497.Admin.Controllers
{
    public class OrderController : Controller
    {
        // =====================================================
        // Order/Index
        // =====================================================
        public IActionResult Index()
        {
            return View();
        }

        // =====================================================
        // Order/Search
        // =====================================================
        public IActionResult Search()
        {
            return View();
        }

        // =====================================================
        // Order/Create
        // =====================================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order model)
        {
            return View();
        }

        // =====================================================
        // Order/Detail/{id}
        // =====================================================
        public IActionResult Detail(int id)
        {
            return View();
        }

        // =====================================================
        // Order/EditCartItem/{id}?productId={productId}
        // =====================================================
        public IActionResult EditCartItem(int id, int productId)
        {
            return View();
        }

        // =====================================================
        // Order/DeleteCartItem/{id}?productId={productId}
        // =====================================================
        public IActionResult DeleteCartItem(int id, int productId)
        {
            return View();
        }

        // =====================================================
        // Order/ClearCart
        // =====================================================
        public IActionResult ClearCart()
        {
            return View();
        }

        // =====================================================
        // Order/Accept/{id}
        // =====================================================
        public IActionResult Accept(int id)
        {
            return View();
        }

        // =====================================================
        // Order/Shipping/{id}
        // =====================================================
        public IActionResult Shipping(int id)
        {
            return View();
        }

        // =====================================================
        // Order/Finish/{id}
        // =====================================================
        public IActionResult Finish(int id)
        {
            return View();
        }

        // =====================================================
        // Order/Reject/{id}
        // =====================================================
        public IActionResult Reject(int id)
        {
            return View();
        }

        // =====================================================
        // Order/Cancel/{id}
        // =====================================================
        public IActionResult Cancel(int id)
        {
            return View();
        }

        // =====================================================
        // Order/Delete/{id}
        // =====================================================
        public IActionResult Delete(int id)
        {
            return View();
        }
    }
}
