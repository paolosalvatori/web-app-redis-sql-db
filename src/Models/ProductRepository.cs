#region Copyright
//=======================================================================================
//Microsoft Windows Server AppFabric Product Advisory Team (CAT)  
//
// This sample is supplemental to the technical guidance published on the community
// blog at http://blogs.msdn.com/b/paolos/. 
// 
// Author: Paolo Salvatori
//=======================================================================================
// Copyright © 2011 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ProductStore.Helpers;
using StackExchange.Redis;

#endregion

namespace ProductStore.Models
{
    public static class ProductRepository
    {
        #region Private Constants
        private const string Keys = "Keys";
        private const string SqlConnectionStringSetting = "SQL_DB_CONNECTION_STRING";
        private const string RedisConnectionStringSetting = "REDIS_CACHE_CONNECTION_STRING";
        #endregion

        #region Private Static Fields
        private static ConnectionMultiplexer connection;
        private static readonly string SqlConnectionString;
        private static readonly string RedisConnectionString;
        #endregion

        #region Static Constructor
        static ProductRepository()
        {
            try
            {
                SqlConnectionString = ConfigurationManager.AppSettings[SqlConnectionStringSetting];
                RedisConnectionString = ConfigurationManager.AppSettings[RedisConnectionStringSetting];
                TraceHelper.TraceInfo("[SqlConnectionStringSetting]: {0} [RedisConnectionStringSetting]: {1}", SqlConnectionString, RedisConnectionString);
                connection = ConnectionMultiplexer.Connect(RedisConnectionString);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(ex.Message);
                throw;
            }
        }

        #endregion

        #region Private Static Properties
        private static ConnectionMultiplexer Connection
        {
            get
            {
                if (connection == null || !connection.IsConnected)
                {
                    connection = ConnectionMultiplexer.Connect(RedisConnectionString);
                }
                return connection;
            }
        }
        #endregion

        #region Public Methods
        public static async Task<Product> GetProductAsync(int id)
        {
            try
            {
                var idAsString = id.ToString(CultureInfo.InvariantCulture);
                var cache = Connection.GetDatabase();
                var item = cache.Get<Product>(idAsString);
                if (item != null)
                {
                    return item;
                }
                using (var sqlConnection = new SqlConnection(SqlConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (var command = new SqlCommand("GetProduct", sqlConnection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 60;
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@ProductID",
                            Direction = ParameterDirection.Input,
                            SqlDbType = SqlDbType.Int,
                            Value = id
                        });
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            var product = new Product
                            {
                                ProductId = await reader.GetFieldValueAsync<int>(0),
                                Name = await reader.IsDBNullAsync(1) ? string.Empty : await reader.GetFieldValueAsync<string>(1),
                                Category = await reader.IsDBNullAsync(2) ? string.Empty : await reader.GetFieldValueAsync<string>(2),
                                Price = await reader.IsDBNullAsync(3) ? 0 : await reader.GetFieldValueAsync<decimal>(3)
                            };
                            var task1 = cache.SetAsync(idAsString, product);
                            var task2 = cache.SetAddAsync(Keys, idAsString);
                            Task.WaitAll(task1, task2);
                            return product;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                ProductEventSource.Log.ExceptionOccurred(ex.Message, ex.InnerException?.Message ?? string.Empty);
                TraceHelper.TraceError(ex.Message);
                throw;
            }
        }

        public static async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                TraceHelper.TraceInfo("GetProductsAsync");
                var cache = Connection.GetDatabase();
                var values = await cache.SetMembersAsync(Keys);
                var items = await cache.GetAsync<Product>(values.Select(v => (string)v).ToArray());
                var list = items.ToList();
                if (list.Any())
                {
                    list.Sort((x, y) => x.ProductId - y.ProductId);
                    return list;
                }
                using (var sqlConnection = new SqlConnection(SqlConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (var command = new SqlCommand("GetProducts", sqlConnection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 60;
                        var reader = await command.ExecuteReaderAsync();
                        var productList = new List<Product>();
                        while (await reader.ReadAsync())
                        {
                            var product = new Product
                            {
                                ProductId = await reader.GetFieldValueAsync<int>(0),
                                Name = await reader.IsDBNullAsync(1) ? string.Empty : await reader.GetFieldValueAsync<string>(1),
                                Category = await reader.IsDBNullAsync(2) ? string.Empty : await reader.GetFieldValueAsync<string>(2),
                                Price = await reader.IsDBNullAsync(3) ? 0 : await reader.GetFieldValueAsync<decimal>(3)
                            };
                            var idAsString = product.ProductId.ToString(CultureInfo.InvariantCulture);
                            productList.Add(product);
                            await cache.SetAsync(idAsString, product);
                            await cache.SetAddAsync(Keys, idAsString);
                        }
                        return productList;
                    }
                }
            }
            catch (Exception ex)
            {
                ProductEventSource.Log.ExceptionOccurred(ex.Message, ex.InnerException?.Message ?? string.Empty);
                TraceHelper.TraceError(ex.Message);
                throw;
            }
        }

        public static async Task<int> DeleteProductAsync(int id)
        {
            try
            {
                TraceHelper.TraceInfo($"DeleteProductAsync: Id=[{id}]");
                var idAsString = id.ToString(CultureInfo.InvariantCulture);
                var cache = Connection.GetDatabase();
                await cache.KeyDeleteAsync(idAsString);
                await cache.SetRemoveAsync(Keys, idAsString);
                using (var sqlConnection = new SqlConnection(SqlConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (var command = new SqlCommand("DeleteProduct", sqlConnection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 60;
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@ProductID",
                            Direction = ParameterDirection.Input,
                            SqlDbType = SqlDbType.Int,
                            Value = id
                        });
                        var i = await command.ExecuteNonQueryAsync();
                        ProductEventSource.Log.ProductDeleted(id);
                        return i;
                    }
                }
            }
            catch (Exception ex)
            {
                ProductEventSource.Log.ExceptionOccurred(ex.Message, ex.InnerException?.Message ?? string.Empty);
                TraceHelper.TraceError(ex.Message);
                throw;
            }
        }

        public static async Task<Product> AddProductAsync(Product product)
        {
            try
            {
                if (product == null)
                {
                    throw new ArgumentException("The product parameter cannot be null.");
                }
                if (product.Price < 0)
                {
                    throw new ArgumentException("The product price cannot be negative.");
                }
                TraceHelper.TraceInfo($"AddProductAsync: Name=[{product.Name}] Category=[{product.Category}] Price=[{product.Price}]");
                using (var sqlConnection = new SqlConnection(SqlConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (var command = new SqlCommand("AddProduct", sqlConnection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 60;
                        var productIdParam = new SqlParameter
                            {
                                ParameterName = "@ProductID",
                                Direction = ParameterDirection.Output,
                                SqlDbType = SqlDbType.Int
                            };
                        command.Parameters.Add(productIdParam);
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@Name",
                            Direction = ParameterDirection.Input,
                            SqlDbType = SqlDbType.NVarChar,
                            Size = 50,
                            Value = product.Name
                        });
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@Category",
                            Direction = ParameterDirection.Input,
                            SqlDbType = SqlDbType.NVarChar,
                            Size = 50,
                            Value = product.Category
                        });
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@Price",
                            Direction = ParameterDirection.Input,
                            SqlDbType = SqlDbType.SmallMoney,
                            Value = product.Price
                        });
                        await command.ExecuteNonQueryAsync();
                        product.ProductId = (int)productIdParam.Value;
                        var idAsString = product.ProductId.ToString(CultureInfo.InvariantCulture);
                        var cache = Connection.GetDatabase();
                        await cache.SetAsync(idAsString, product);
                        await cache.SetAddAsync(Keys, idAsString);
                        ProductEventSource.Log.ProductAdded(product.ProductId, 
                                                            product.Name,
                                                            product.Category,
                                                            (double)product.Price);
                        return product;
                    }
                }
            }
            catch (Exception ex)
            {
                ProductEventSource.Log.ExceptionOccurred(ex.Message, ex.InnerException?.Message ?? string.Empty);
                TraceHelper.TraceError(ex.Message);
                throw;
            }
        }

        public static async Task<Product> UpdateProductAsync(Product product)
        {
            try
            {
                if (product == null)
                {
                    throw new ArgumentException("The product parameter cannot be null.");
                }
                if (product.Price < 0)
                {
                    throw new ArgumentException("The product price cannot be negative.");
                }
                TraceHelper.TraceInfo($"UpdateProductAsync: Id=[{product.ProductId}] Name=[{product.Name}] Category=[{product.Category}] Price=[{product.Price}]");
                using (var sqlConnection = new SqlConnection(SqlConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (var command = new SqlCommand("UpdateProduct", sqlConnection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 60;
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@ProductID",
                            Direction = ParameterDirection.Input,
                            SqlDbType = SqlDbType.Int,
                            Value = product.ProductId
                        });
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@Name",
                            Direction = ParameterDirection.Input,
                            SqlDbType = SqlDbType.NVarChar,
                            Size = 50,
                            Value = product.Name
                        });
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@Category",
                            Direction = ParameterDirection.Input,
                            SqlDbType = SqlDbType.NVarChar,
                            Size = 50,
                            Value = product.Category
                        });
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@Price",
                            Direction = ParameterDirection.Input,
                            SqlDbType = SqlDbType.SmallMoney,
                            Value = product.Price
                        });
                        await command.ExecuteNonQueryAsync();
                        var idAsString = product.ProductId.ToString(CultureInfo.InvariantCulture);
                        var cache = Connection.GetDatabase();
                        await cache.SetAsync(idAsString, product);
                        await cache.SetAddAsync(Keys, idAsString);
                        ProductEventSource.Log.ProductUpdated(product.ProductId,
                                                            product.Name,
                                                            product.Category,
                                                            (double)product.Price);
                        return product;
                    }
                }
            }
            catch (Exception ex)
            {
                ProductEventSource.Log.ExceptionOccurred(ex.Message, ex.InnerException?.Message ?? string.Empty);
                TraceHelper.TraceError(ex.Message);
                throw;
            }
        }
        #endregion
    }
}
