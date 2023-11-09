using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models
{
	public class OrderHeader
	{
		public int Id { get; set; }

		public string ApplicationUserId { get; set; }

		[ForeignKey(nameof(ApplicationUserId))]
		[ValidateNever]
		public ApplicationUser ApplicationUser { get; set; }

		[Required]
		public DateTime OrderDate{ get; set; }
		public DateTime ShippingDate { get; set; }

		public double OrderTotal { get; set; }

		public string? OrderStatus { get; set; }
		public string? PaymentStatus { get; set; }

        public string?  TrackingNumber { get; set; }
		public string? Carrer { get; set; }

		public DateTime PaymentDate { get; set; }
		public DateTime PaymentDueDate { get; set; }

		public string? SessionId { get; set; }
		public string? PaymentIntentNumber { get; set; }

		[Required]
		public String Name { get; set; }
		[Required]
		public string PhoneNumber { get; set; }

		[Required]
		public String City { get; set; }

		[Required]
		public String Street { get; set; }

		[Required]
		public String StreetAddress { get; set; }

		[Required]
		public String PostalCode { get; set; }

	}


}
