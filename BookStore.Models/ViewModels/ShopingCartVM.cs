using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModels
{
	public class ShopingCartVM
	{
		public IEnumerable<ShopingCart> ListCart {  get; set; }
		public double CartTotal { get; set; }
	}
}
