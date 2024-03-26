using System.ComponentModel.DataAnnotations;
using System.Security.Policy;


namespace EcoPlanet.Models
{
	public class Goods
	{
		[Key]
		public int goodsId { get; set; }

		public string ? goodsType { get; set; }

		public string ? goodsName { get; set; }

		public string ? goodsDescriptions { get; set;}


		public decimal goodsPrice { get; set; }

		
		public int goodsQuantity {  get; set; }	


		public DateTime goodsExpiry { get; set; }


		public string ? goodsStatus { get; set; }

		public string ? goodsImage { get; set; }


		public string ? SellerId { get; set; }


	}
}
