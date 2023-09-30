using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models
{
	public class Company
	{
        public int Id { get; set; }

        [Required]
        public String Name { get; set; }
        public String? StreetAddress { get; set; }

        public string? Street { get; set; }
        public String? City { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
