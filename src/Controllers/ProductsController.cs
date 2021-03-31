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
using System;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Products.Models;
using Products.Properties;
using Products.Helpers;
#endregion

namespace Products.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        #region Private Instance Fields
        private readonly ILogger<ProductsController> logger;
        private readonly ProductsContext context;
        private readonly IDatabase database;
        #endregion

        #region Public Constructors
        public ProductsController(ILogger<ProductsController> logger,
                                  ProductsContext context,
                                  IConnectionMultiplexer connectionMultiplexer)
        {
            this.logger = logger;
            this.context = context;
            database = connectionMultiplexer.GetDatabase();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets all the products.
        /// </summary>
        /// <returns>All the products.</returns>
        /// <response code="200">Get all the products, if any.</response>
        [HttpGet]
        [ProducesResponseType(typeof(Product), 200)]
        public async Task<IActionResult> GetAllProductsAsync()
        {
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();
                logger.LogInformation("Listing all products...");
                var values = await database.SetMembersAsync(Resources.RedisKeys);
                var items = await database.GetAsync<Product>(values.Select(v => (string)v).ToArray());
                if (items.Any())
                {
                    var list = items.ToList();
                    list.Sort((x, y) => x.ProductId - y.ProductId);
                    return new OkObjectResult(list.ToArray());
                }
                var products = context.Products.FromSqlRaw(Resources.GetProducts);
                foreach (var product in products)
                {
                    var idAsString = product.ProductId.ToString(CultureInfo.InvariantCulture);
                    await database.SetAsync(idAsString, product);
                    await database.SetAddAsync(Resources.RedisKeys, idAsString);
                }
                return new OkObjectResult(products.ToArray());
            }
            catch (Exception ex)
            {
                var errorMessage = MessageHelper.FormatException(ex);
                logger.LogError(errorMessage);
                return StatusCode(400, new { error = errorMessage });
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation($"GetAllProductsAsync method completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        /// <summary>
        /// Gets a specific product by id.
        /// </summary>
        /// <param name="id">Id of the product.</param>
        /// <returns>Product with the specified id.</returns>
        /// <response code="200">Product found</response>
        /// <response code="404">Product not found</response>     
        [HttpGet("{id}", Name = "GetProductByIdAsync")]
        [ProducesResponseType(typeof(Product), 200)]
        [ProducesResponseType(typeof(Product), 404)]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();
                logger.LogInformation($"Getting product {id}...");
                var product = await database.GetAsync<Product>(id.ToString());
                if (product != null)
                {
                    return new OkObjectResult(product);
                }

                var products = context.Products.FromSqlRaw(Resources.GetProduct, new SqlParameter
                {
                    ParameterName = "@ProductID",
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.Int,
                    Value = id
                });
                if (products.Any())
                {
                    product = products.FirstOrDefault();
                    var idAsString = product.ProductId.ToString(CultureInfo.InvariantCulture);
                    await database.SetAsync(idAsString, product);
                    await database.SetAddAsync(Resources.RedisKeys, idAsString);

                    logger.LogInformation($"Product with id = {product.ProductId} has been successfully retrieved.");
                    return new OkObjectResult(product);
                }
                else
                {
                    logger.LogWarning($"No product with id = {id} was found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = MessageHelper.FormatException(ex);
                logger.LogError(errorMessage);
                return StatusCode(400, new { error = errorMessage });
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation($"GetProductByIdAsync method completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="product">Product to create.</param>
        /// <returns>If the operation succeeds, it returns the newly created product.</returns>
        /// <response code="201">Product successfully created.</response>
        /// <response code="400">Product is null.</response>     
        [HttpPost]
        [ProducesResponseType(typeof(Product), 201)]
        [ProducesResponseType(typeof(Product), 400)]
        public async Task<IActionResult> CreateProductAsync(Product product)
        {
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();
                if (product == null)
                {
                    logger.LogWarning("Product cannot be null.");
                    return BadRequest();
                }

                var productIdParameter = new SqlParameter
                {
                    ParameterName = "@ProductID",
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.Int
                };

                var result = await context.Database.ExecuteSqlRawAsync(Resources.AddProduct, new SqlParameter[] {
                    productIdParameter,
                    new SqlParameter
                    {
                        ParameterName = "@Name",
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 50,
                        Value = product.Name
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Category",
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 50,
                        Value = product.Category
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Price",
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.SmallMoney,
                        Value = product.Price
                    }
                });
                if (result ==1 && productIdParameter.Value != null)
                {
                    product.ProductId = (int)productIdParameter.Value;
                    var idAsString = product.ProductId.ToString(CultureInfo.InvariantCulture);
                    await database.SetAsync(idAsString, product);
                    await database.SetAddAsync(Resources.RedisKeys, idAsString);
                    
                    logger.LogInformation($"Product with id = {product.ProductId} has been successfully created.");
                    return CreatedAtRoute("GetProductByIdAsync", new { id = product.ProductId }, product);
                }
                return null;
            }
            catch (Exception ex)
            {
                var errorMessage = MessageHelper.FormatException(ex);
                logger.LogError(errorMessage);
                return StatusCode(400, new { error = errorMessage });
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation($"CreateProductAsync method completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        /// <summary>
        /// Updates a product. 
        /// </summary>
        /// <param name="id">The id of the product.</param>
        /// <param name="product">Product to update.</param>
        /// <returns>No content.</returns>
        /// <response code="204">No content if the product is successfully updated.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Product), 204)]
        [ProducesResponseType(typeof(Product), 404)]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();
                if (product == null || product.ProductId != id)
                {
                    logger.LogWarning("The product is null or its id is different from the id in the payload.");
                    return BadRequest();
                }

                var result = await context.Database.ExecuteSqlRawAsync(Resources.UpdateProduct, new SqlParameter[] {
                    new SqlParameter
                    {
                        ParameterName = "@ProductID",
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.Int,
                        Value = product.ProductId
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Name",
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 50,
                        Value = product.Name
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Category",
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 50,
                        Value = product.Category
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Price",
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.SmallMoney,
                        Value = product.Price
                    }
                });

                if (result == 1)
                {
                    var idAsString = id.ToString(CultureInfo.InvariantCulture);
                    await database.SetAsync(idAsString, product);
                    await database.SetAddAsync(Resources.RedisKeys, idAsString);

                    logger.LogInformation("Product with id = {ID} has been successfully updated.", product.ProductId);
                }
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                var errorMessage = MessageHelper.FormatException(ex);
                logger.LogError(errorMessage);
                return StatusCode(400, new { error = errorMessage });
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation($"Update method completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        /// <summary>
        /// Deletes a specific product.
        /// </summary>
        /// <param name="id">The id of the product.</param>      
        /// <returns>No content.</returns>
        /// <response code="202">No content if the product is successfully deleted.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Product), 204)]
        [ProducesResponseType(typeof(Product), 404)]
        public async Task<IActionResult> Delete(string id)
        {
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                var result = await context.Database.ExecuteSqlRawAsync(Resources.DeleteProduct, new SqlParameter[] {
                    new SqlParameter
                    {
                        ParameterName = "@ProductID",
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.Int,
                        Value = id
                    }
                });

                if (result == 1)
                {
                    var idAsString = id.ToString(CultureInfo.InvariantCulture);
                    await database.KeyDeleteAsync(idAsString);
                    await database.SetRemoveAsync(Resources.RedisKeys, idAsString);

                    logger.LogInformation("Product with id = {ID} has been successfully deleted.", id);
                }
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                var errorMessage = MessageHelper.FormatException(ex);
                logger.LogError(errorMessage);
                return StatusCode(400, new { error = errorMessage });
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation($"Delete method completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }
        #endregion
    }
}
