using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository.IRepository
{
    public interface IShopingCartRepository : IRepository<ShopingCart>
    {
        int UpdateCount (ShopingCart shopingCart,int count);
        int IncrementCount (ShopingCart shopingCart, int count);

        int DecrementCount (ShopingCart shopingCart, int count);
    }
}
