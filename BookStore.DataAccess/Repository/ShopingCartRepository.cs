using BookStore.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class ShopingCartRepository : Repository<ShopingCart>, IShopingCartRepository
    {
        public ApplicationDbContext _db;

        public ShopingCartRepository(ApplicationDbContext db) :base(db)
        {
            _db = db;
        }

		public int DecrementCount(ShopingCart shopingCart, int count)
		{
            shopingCart.Count -= count;
            return shopingCart.Count;
		}

		public int IncrementCount(ShopingCart shopingCart, int count)
		{
			shopingCart.Count += count;
			return shopingCart.Count;
		}

		public int UpdateCount(ShopingCart shopingCart, int count)
        {
                shopingCart.Count = count;
                return shopingCart.Count;
            }
        }

        
    }

