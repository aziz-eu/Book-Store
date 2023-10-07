using BookStore.Data;
using BookStore.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {

        public ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository (_db);
            CoverType = new CoverTypeRepository (_db);
            Product = new ProductRepository (_db);
            Company = new CompanyRepository (_db);
            ShopingCart = new ShopingCartRepository (_db);
        }

        public ICategoryRepository Category { get; private set; }

        public ICoverTypeRepository CoverType {  get; private set; }

        public IProductRepository Product {  get; private set; }

        public ICompanyRepository Company { get; private set; }
       
        public IShopingCartRepository ShopingCart { get; private set; }

      

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
