﻿using DutchTreat.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DutchTreat.Data
{
    public class DutchSeeder
    {
        private readonly DutchContext ctx;
        private readonly IHostingEnvironment hosting;
        private readonly UserManager<StoreUser> userManager;

        public DutchSeeder(DutchContext ctx, IHostingEnvironment hosting, UserManager<StoreUser> userManager)
        {
            this.ctx = ctx;
            this.hosting = hosting;
            this.userManager = userManager;
        }

        public async Task Seed()
        {
            ctx.Database.EnsureCreated();

            var user = await userManager.FindByEmailAsync("scott@scott.com");

            if (user == null)
            {
                user = new StoreUser()
                {
                    FirstName = "Scott",
                    LastName = "Shumway",
                    UserName = "scott@scott.com",
                    Email = "scott@scott.com"
                };

                var result = await userManager.CreateAsync(user, "P@ssw0rd!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create deafult user");
                }
            }

            if (!ctx.Products.Any())
            {
                // need to create sample data

                var filepath = Path.Combine(hosting.ContentRootPath, "Data/art.json");
                var json = File.ReadAllText(filepath);

                var products = JsonConvert.DeserializeObject<IEnumerable<Product>>(json);

                ctx.Products.AddRange(products);

                var order = new Order()
                {
                    OrderDate = DateTime.Now,
                    OrderNumber = "1234",
                    User = user,
                    Items = new List<OrderItem>()
                    {
                        new OrderItem()
                        {
                            Product = products.First(),
                            Quantity = 5,
                            UnitPrice = products.First().Price
                        }
                    }
                };

                ctx.Orders.Add(order);

                ctx.SaveChanges();
            }
        }
    }
}
