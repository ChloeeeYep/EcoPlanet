using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using EcoPlanet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EcoPlanet.Areas.Identity.Data;


namespace EcoPlanet.Controllers
{
	public class GoodsController : Controller
	{
		private readonly EcoPlanetContext _context;
		private readonly UserManager<EcoPlanetUser> _userManager;

		public GoodsController(EcoPlanetContext context, UserManager<EcoPlanetUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User); // Gets the current logged-in user
			if (user == null)
			{
				return RedirectToAction("Login", "Account");
			}

			var email = user.Email; 
			var goods = await _context.GoodsTable
									  .Where(g => g.SellerId == email)
									  .ToListAsync();

			return View(goods);
		}

		public IActionResult AddGoods()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddGoods(Goods goods)
		{
			if(ModelState.IsValid)
			{
				_context.GoodsTable.Add(goods);
				await _context.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			return View(goods);			

		}

		public async Task<IActionResult> EditGoods(int ? goodsId)
		{
			if(goodsId == null)
			{
				return NotFound();
			}
			var goods = await _context.GoodsTable.FindAsync(goodsId);

			if(goods == null)
			{
				return BadRequest(goodsId + " is not found in the table");
			}
			return View(goods);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateGoods(Goods goods)
		{
			try
			{
				if(ModelState.IsValid)
				{
					_context.GoodsTable.Update(goods);
					await _context.SaveChangesAsync();
					return RedirectToAction("Index","Goods");
					
				}
				return View("EditGoods", goods);
			}
			catch (Exception ex)
			{
				return BadRequest("Error: " + ex.Message);
			}
		}

		public async Task<IActionResult> DeleteGoods(int ? goodsId)
		{
            if (goodsId == null)
            {
				return NotFound();
            }

			var goods = await _context.GoodsTable.FindAsync(goodsId);
			if(goods == null)
			{
				return BadRequest(goodsId + " is not found from the lists");
			}

			_context.GoodsTable.Remove(goods);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", "Goods");

        }
	}
}
