#region Copyright
//=======================================================================================
// Author: Paolo Salvatori
// GitHub: https://github.com/paolosalvatori
//=======================================================================================
// Copyright © 2021 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
#endregion

#region Using Directives
using Microsoft.EntityFrameworkCore;
using System;
#endregion

namespace Products.Models
{
    /// <summary>
    /// ProductsDbContext class
    /// </summary>
    public class ProductsContext : DbContext
    {
        #region Public Constructor
        /// <summary>
        /// Public Constructor
        /// </summary>
        /// <param name="options">DbContextOptions object</param>
        public ProductsContext(DbContextOptions<ProductsContext> options)
            : base(options)
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the Products property
        /// </summary>
        public DbSet<Product> Products { get; set; }
        #endregion

        #region Protected Methods
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .LogTo(Console.WriteLine); 
        #endregion
    }
}
