using BookStore.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
       
        private readonly UserManager<IdentityUser> _userManager; 
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public DbInitializer (
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db
            )


        {
         
            _userManager = userManager;          
            _roleManager = roleManager;
            _db = db;
        }


        // migration, if not apply
        public void Initializer()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }

            }
            catch ( Exception ex )
            {

            }



            // Role create if they are not created

            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Compnay)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Individual)).GetAwaiter().GetResult();


                // if Role is not created then create admin
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@bookstore.com",
                    Email = "admin@bookstore.com",
                    Name = "Admin",
                    PhoneNumber = "01842605509",
                    Street = "Mohammadpur",
                    StreetAddress = "SKK, RD#11 H#07 ",
                    City = "Chikago"

                }, "@Dmin123"
                    ).GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@bookstore.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return ;
             
        }
    }
}
