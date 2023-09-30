using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models
{
	public class ApplicationUser : IdentityUser
	{
        public String Name { get; set; }
		public String? City {  get; set; }

		public String? Street { get; set; }

		public String? StreetAddress { get; set; }
		public String? PostalCode { get; set; }
	}
}
