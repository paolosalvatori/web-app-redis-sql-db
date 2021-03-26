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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ProductStore.Models; 
#endregion

namespace ProductStore.Controllers
{
    public class ProductsController : ApiController
    {
        #region Public Methods
        [HttpGet]
        public async Task<IEnumerable<Product>> RetreiveAllProducts()
        {
            try
            {
                return await ProductRepository.GetProductsAsync();
            }
            catch (Exception ex)
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(ex.Message),
                        ReasonPhrase = ex.Message
                        //ReasonPhrase = "An error occurred while retrieving products from the repository."
                    };
                throw new HttpResponseException(httpResponseMessage);
            }
        }

        public async Task<Product> GetProduct(int id)
        {
            try
            {
                var item = await ProductRepository.GetProductAsync(id);
                if (item == null)
                {
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            Content = new StringContent($"No product with ID = {id} was found."),
                            ReasonPhrase = "Product ID Not Found"
                        };
                    throw new HttpResponseException(httpResponseMessage);
                }
                return item;
            }
            catch (Exception ex)
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "An error occurred while retrieving the product from the repository."
                };
                throw new HttpResponseException(httpResponseMessage);
            }
        }

        //public async Task<IEnumerable<Product>> GetProductByCategory(string category)
        //{
        //    try
        //    {
        //        return await ProductRepository.GetProductsByCategoryAsync(category);
        //    }
        //    catch (Exception ex)
        //    {
        //        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        //        {
        //            Content = new StringContent(ex.Message),
        //            ReasonPhrase = "An error occurred while retrieving products from the repository."
        //        };
        //        throw new HttpResponseException(httpResponseMessage);
        //    }
        //}

        public async Task<HttpResponseMessage> PostProduct(Product item)
        {
            try
            {
                item = await ProductRepository.AddProductAsync(item);
                var response = Request.CreateResponse(HttpStatusCode.Created, item);

                var uri = Url.Link("DefaultApi", new { id = item.ProductId });
                if (uri != null)
                {
                    response.Headers.Location = new Uri(uri);
                }
                return response;
            }
            catch (Exception ex)
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "An error occurred while inserting the product into the repository."
                };
                throw new HttpResponseException(httpResponseMessage);
            }
        }

        public async Task PutProduct(int id, Product item)
        {
            try
            {
                item.ProductId = id;
                if (await ProductRepository.UpdateProductAsync(item) == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "An error occurred while updating the product on the repository."
                };
                throw new HttpResponseException(httpResponseMessage);
            }
        }

        public async Task DeleteProduct(int id)
        {
            try
            {
                if (await ProductRepository.DeleteProductAsync(id) == 0)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "An error occurred while deleting the product from the repository."
                };
                throw new HttpResponseException(httpResponseMessage);
            }
        }
        #endregion
    }
}
