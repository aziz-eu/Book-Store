﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class ShopingCart
    {
        public Product Product { get; set; }

        [Range(1, 1000)]
        public int Count { get; set; }
    }
}
