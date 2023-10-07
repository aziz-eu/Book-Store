﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class ShopingCart
    {
        public int Id { get; set; }

        public int  ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        public string ApplicationUserId {  get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        [Range(1, 1000)]
        public int Count { get; set; }

        [NotMapped]
        public double Price { get; set; }
    }
}
