﻿using ProniaAB104.Models;

namespace ProniaAB104.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
    }
}
