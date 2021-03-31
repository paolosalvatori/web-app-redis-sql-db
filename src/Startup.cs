#region Copyright
//=======================================================================================
// Author: Paolo Salvatori
// GitHub: https://github.com/paolosalvatori
//=======================================================================================
// Copyright � 2021 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
#endregion

#region Using Directives
using System;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Products.Properties;
using Products.Models;
using Products.Helpers;
#endregion

namespace Products
{
    public class Startup
    {
        /// <summary>
        /// Creates an instance of the Startup class
        /// </summary>
        /// <param name="configuration">The configuration created by the CreateDefaultBuilder.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets or sets the Configuration property.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The services collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddApplicationInsightsTelemetry(Configuration[Resources.ApplicationInsightsConnectionString]);
            services.AddOptions();
            services.AddMvc();
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(Configuration.GetConnectionString(Resources.RedisCacheConnectionString)));
            services.AddDbContext<ProductsContext>(options => options.UseSqlServer(Configuration.GetConnectionString(Resources.SqlServerConnectionString)));

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Products API",
                    Description = "A simple example ASP.NET Core Web API",
                    TermsOfService = new Uri("https://www.apache.org/licenses/LICENSE-2.0"),
                    Contact = new OpenApiContact
                    {
                        Name = "Paolo Salvatori",
                        Email = "paolos@microsoft.com",
                        Url = new Uri("https://github.com/paolosalvatori")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under Apache License 2.0",
                        Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0")
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoList API V1");
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
