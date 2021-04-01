---
products: azure,  aspnet, azure-application-insights, azure-app-service, azure-blob-storage, azure-storage-accounts, azure-sql, azure-cache-for-redis, azure-database, azure-functions, azure-log-analytics, azure-nat-gateway, azure-virtual-machines, vs-code
---

# How to configure a Web App to call Azure Cache for Redis and Azure SQL Database via Private Endpoints

This sample shows how to configure a Web App running in an [Azure App Service](https://docs.microsoft.com/en-us/azure/app-service/) an [HTTP-triggered Azure Web App](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=csharp) to access [Azure Cache for Redis](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview) and [Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview) using [Azure Private Endpoints](https://docs.microsoft.com/azure/private-link/private-endpoint-overview). The Azure Web Apps app is hosted in [Azure Web Apps Premium Plan](https://docs.microsoft.com/en-us/azure/azure-functions/functions-premium-plan?tabs=portal) with [Regional VNET Integration](https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet#regional-vnet-integration).
Private endpoints are fully supported also by the Standard tier of Azure Cache for Redis. However, to use private endpoints, an Azure Cache for Redis instance needs to have been created after July 28th, 2020. Currently, geo-replication, firewall rules, portal console support, multiple endpoints per clustered cache, persistence to firewall and VNet injected caches is not supported.
As an alternative solution, this sample also shows how to deploy Premium Azure Cache for Redis in a virtual neytwork. When an Azure Cache for Redis instance is configured with a virtual network is not publicly addressable and can only be accessed from virtual machines and applications within the virtual network or a peered virtual network.

For more information, see:

- [Azure Private Link for Azure SQL Database and Azure Synapse Analytics](https://docs.microsoft.com/en-us/azure/azure-sql/database/private-endpoint-overview)
- [Web app private connectivity to Azure SQL database](https://docs.microsoft.com/en-us/azure/architecture/example-scenario/private-web-app/private-web-app)
- [Azure Cache for Redis with Azure Private Link](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-private-link)
- [Configure virtual network support for a Premium Azure Cache for Redis instance](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-premium-vnet)

In addition, Azure Web Apps and Http-triggered Azure Web Apps can be configured to be called via a private IP address by applications located in the same virtual network, or in a peered network, or on-premises via ExpressRoute or a S2S VPN. For more information, see:

- [Using Private Endpoints for Azure Web App](https://docs.microsoft.com/en-us/azure/app-service/networking/private-endpoint).
- [Create an App Service app and deploy a private endpoint by using an Azure Resource Manager template](https://docs.microsoft.com/en-us/azure/app-service/scripts/template-deploy-private-endpoint).
- [Call an HTTP Azure Web App using a Private Endpoint](https://github.com/paolosalvatori/azure-functions-private-endpoint-http-trigger)

For a similar sample with a non-HTTP-triggered Azure Web App, see [Azure Web Apps, Private Endpoints, and NAT Gateway](https://github.com/paolosalvatori/azure-function-premium-plan).

This sample shows also how to disable the public network access from the internet to all the managed services used by the application:

- Azure Blob Storage Account
- Azure Key Vault
- Azure Cache for Redis
- Azure SQL Database

## Deploy to Azure

You can use the following button to deploy the demo to your Azure subscription:

Azure Cache for Redis via Private Endpoints

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fpaolosalvatori%2Fweb-app-redis-sql-db%2Fmaster%2Ftemplates%2Fazuredeploy.endpoint.json%3Ftoken%3DAAIW4AOWATWNQLL2JZKDBAK63EOOU)

Azure Cache for Redis in a virtual network

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fpaolosalvatori%2Fweb-app-redis-sql-db%2Fmaster%2Ftemplates%2Fazuredeploy.endpoint.json%3Ftoken%3DAAIW4AOWATWNQLL2JZKDBAK63EOOU)

## Architecture

The following picture shows the architecture and network topology of the first solution where a Standard Azure Cache for Redis is accessed by an Azure Web App via [Regional VNET Integration](https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet#regional-vnet-integration) and [Azure Private Endpoints](https://docs.microsoft.com/azure/private-link/private-endpoint-overview).

![Architecture with Azure Cache for Redis accessed via Private Endpoint](images/redis-cache-private-endpoint.png)

The ARM template deploys the following resources:

- Virtual Network: this virtual network is composed of the following subnets:
  - **WebAppSubnet**: this subnet is used for the regional VNET integration with the Azure Web App app hosted by a Premium Plan. For more information, see [Using Private Endpoints for Azure Web App](https://docs.microsoft.com/en-us/azure/app-service/networking/private-endpoint).
  - **PrivateEndpointSubnet**: hosts the private endpoints used by the application.
  - **VirtualMachineSubnet**: hosts the jumpbox virtual machine and any additional virtual machine used by the solution.
  - **AzureBastionSubnet**: hosts Azure Bastion. For more information, see [Working with NSG access and Azure Bastion](https://docs.microsoft.com/en-us/azure/bastion/bastion-nsg).
- Network Security Group: this resource contains an inbound rule to allow access to the jumpbox virtual machine on port 3389 (RDP)
- A Windows 10 virtual machine. This virtual machine can be used as jumpbox virtual machine to simulate a real application and send requests to the Azure Web Apps exposed via [Azure Private Link](https://docs.microsoft.com/en-us/azure/private-link/private-link-overview).
- A Public IP for Azure Bastion
- Azure Bastion is used to access the jumpbox virtual machine from the Azure Portal via RDP. For more information, see [What is Azure Bastion?](https://docs.microsoft.com/en-us/azure/bastion/bastion-overview).
- An ADLS Gen 2 storage account used to store the boot diagnostics logs of the virtual machine as blobs
- An ADLS Gen 2 storage account where the code and configuration of the Azure Web App are stored. For more information, see [WEBSITE_CONTENTAZUREFILECONNECTIONSTRING](https://docs.microsoft.com/it-it/azure/azure-functions/functions-app-settings#website_contentazurefileconnectionstring).
- An Premium App Service Plan hosting the Azure Web App app. For more information, see [Azure App Service plan overview](https://docs.microsoft.com/en-us/azure/app-service/overview-hosting-plans).
- An Azure App Service containing an ASP.NET Core application that uses a system-assigned managed identity to read settings from Key vault, stores data in [Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview), and caches items in Azure Cache for Redis.
- An Application Insights resource used by the Azure Web Apps app to store logs, traces, requests, exceptions, and metrics. For more information, see [Monitor Azure Web Apps](https://docs.microsoft.com/en-us/azure/azure-functions/functions-monitoring).
- An Azure SQL Server and [Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview) hosting the ProductDB relational database used by the Web App.
- An Azure Key Vault used to store the following application settings. These settings are automtically created by the ARM template as secrets in Azure Key Vault:

  - Azure Cache for Redis connection string
  - Azure SQL Database connection string
  - Application Insights Instrumentation Key

- A private endpoint to the:

  - Azure Blob storage account (boot diagnostics logs)
  - Azure Cache for Redis
  - Azure SQL Database
  - Azure Key Vault

- A Private DNS Zone Group to link each private endpoint with the corresponding Private DNS Zone.
- The NIC used by the jumpbox virtual machine and for each private endpoint.
- A Log Analytics workspace used to monitor the health status of the services such as the hosting plan or NSG.
- A Private DNS Zone for Azure Blob Storage Account private endpoint (privatelink.blob.core.windows.net)
- A Private DNS Zone for Azure Cache for Redis private endpoint (privatelink.redis.cache.windows.net)
- A Private DNS Zone for Azure SQL Database private endpoint (privatelink.database.windows.net)
- A Private DNS Zone for Azure Key Vault private endpoint (privatelink.vaultcore.azure.net)

The following picture shows the architecture and network topology of the first solution where a Standard Azure Cache for Redis is accessed by an Azure Web App via [Regional VNET Integration](https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet#regional-vnet-integration) and [Azure Private Endpoints](https://docs.microsoft.com/azure/private-link/private-endpoint-overview).

![Architecture with Azure Cache for Redis accessed in a VNET](images/redis-cache-in-a-vnet.png)

The ARM template deploys the following resources:

- Virtual Network: this virtual network is composed of the following subnets:
  - **WebAppSubnet**: this subnet is used for the regional VNET integration with the Azure Web App app hosted by a Premium Plan. For more information, see [Using Private Endpoints for Azure Web App](https://docs.microsoft.com/en-us/azure/app-service/networking/private-endpoint).
  - **PrivateEndpointSubnet**: hosts the private endpoints used by the application.
  - **VirtualMachineSubnet**: hosts the jumpbox virtual machine and any additional virtual machine used by the solution.
  - **AzureBastionSubnet**: hosts Azure Bastion. For more information, see [Working with NSG access and Azure Bastion](https://docs.microsoft.com/en-us/azure/bastion/bastion-nsg).
  - **RedisCacheSubnet**: hosts the Premium Azure Cache for Redis
- Network Security Group: this resource contains an inbound rule to allow access to the jumpbox virtual machine on port 3389 (RDP)
- A Windows 10 virtual machine. This virtual machine can be used as jumpbox virtual machine to simulate a real application and send requests to the Azure Web Apps exposed via [Azure Private Link](https://docs.microsoft.com/en-us/azure/private-link/private-link-overview).
- A Public IP for Azure Bastion
- Azure Bastion is used to access the jumpbox virtual machine from the Azure Portal via RDP. For more information, see [What is Azure Bastion?](https://docs.microsoft.com/en-us/azure/bastion/bastion-overview).
- An ADLS Gen 2 storage account used to store the boot diagnostics logs of the virtual machine as blobs
- An ADLS Gen 2 storage account where the code and configuration of the Azure Web App are stored. For more information, see [WEBSITE_CONTENTAZUREFILECONNECTIONSTRING](https://docs.microsoft.com/it-it/azure/azure-functions/functions-app-settings#website_contentazurefileconnectionstring).
- An Premium App Service Plan hosting the Azure Web App app. For more information, see [Azure App Service plan overview](https://docs.microsoft.com/en-us/azure/app-service/overview-hosting-plans).
- An Azure App Service containing an ASP.NET Core application that uses a system-assigned managed identity to read settings from Key vault, stores data in [Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview), and caches items in Azure Cache for Redis.
- An Application Insights resource used by the Azure Web Apps app to store logs, traces, requests, exceptions, and metrics. For more information, see [Monitor Azure Web Apps](https://docs.microsoft.com/en-us/azure/azure-functions/functions-monitoring).
- An Azure SQL Server and [Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview) hosting the ProductDB relational database used by the Web App.
- An Azure Key Vault used to store the following application settings. These settings are automtically created by the ARM template as secrets in Azure Key Vault:

  - Azure Cache for Redis connection string
  - Azure SQL Database connection string
  - Application Insights Instrumentation Key

- A private endpoint to the:

  - Azure Blob storage account (boot diagnostics logs)
  - Azure SQL Database
  - Azure Key Vault

- A Private DNS Zone Group to link each private endpoint with the corresponding Private DNS Zone.
- The NIC used by the jumpbox virtual machine and for each private endpoint.
- A Log Analytics workspace used to monitor the health status of the services such as the hosting plan or NSG.
- A Private DNS Zone for Azure Blob Storage Account private endpoint (privatelink.blob.core.windows.net)
- A Private DNS Zone for Azure SQL Database private endpoint (privatelink.database.windows.net)
- A Private DNS Zone for Azure Key Vault private endpoint (privatelink.vaultcore.azure.net)

## Important Notes

The two ARM templates disable the public access to both Azure SQL Database and Azure Cache for Redis via the `publicNetworkAccess` parameter which default value is set to `false`. Using private endpoints is not enough to secure an application, you also have to disable the public access to the managed services used by the application, in this case Azure SQL Database and Azure Cache for Redis.

In addition, both ARM templates automatically create the connection string to both the Azure Cache for Redis and Azure SQL Database as application settings of the Azure App Service. However, in a production environment, it's recommended to access adopt one of the following approaches:

- Use a system assigned managed identity from the Web App to access Azure SQL Database. For more information, see [](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-connect-msi). For more information about the Azure Services that support managed identities, see [Services that support managed identities for Azure resources](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/services-support-managed-identities).
- Store sensitive data like connection strings, encryption keys, certificates, and connection string in [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/general/overview). For more information, see [Tutorial: Use a managed identity to connect Key Vault to an Azure web app in .NET](https://docs.microsoft.com/en-us/azure/key-vault/general/tutorial-net-create-vault-azure-web-app).

## Prerequisites

The following components are required to run this sample:

- [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli?view=azure-cli-latest)
- [Azure subscription](https://azure.microsoft.com/free/)

## Topology Deployment

You can use the ARM template and Bash script included in the sample to deploy to Azure the entire infrastructure necessary to host the demo:

```sh
#!/bin/bash

# Clear the screen
clear

# Print the menu
echo "================================================="
echo "Install Demo. Choose an option (1-3): "
echo "================================================="
options=("Inject Premium Azure Cache for Redis in a VNET"
         "Azure Cache for Redis with Azure Private Link"
         "Quit")

# Select an option
COLUMNS=0
select opt in "${options[@]}"; do
    case $opt in
    "Inject Premium Azure Cache for Redis in a VNET")
        template="../templates/azuredeploy.vnet.json"
        parameters="../templates/azuredeploy.vnet.parameters.json"
        resourceGroupName="WebAppSqlDbRedisInVnetRG"
        break
        ;;
    "Azure Cache for Redis with Azure Private Link")
        template="../templates/azuredeploy.endpoint.json"
        parameters="../templates/azuredeploy.endpoint.parameters.json"
        resourceGroupName="WebAppSqlDbRedisCacheRG"
        break
        ;;
    "Quit")
        exit
        ;;
    *) echo "invalid option $REPLY" ;;
    esac
done

# Variables
location="WestEurope"

# SubscriptionId of the current subscription
subscriptionId=$(az account show --query id --output tsv)
subscriptionName=$(az account show --query name --output tsv)

# Check if the resource group already exists
createResourceGroup() {
    local resourceGroupName=$1
    local location=$2

    # Parameters validation
    if [[ -z $resourceGroupName ]]; then
        echo "The resource group name parameter cannot be null"
        exit
    fi

    if [[ -z $location ]]; then
        echo "The location parameter cannot be null"
        exit
    fi

    echo "Checking if [$resourceGroupName] resource group actually exists in the [$subscriptionName] subscription..."

    if ! az group show --name "$resourceGroupName" &>/dev/null; then
        echo "No [$resourceGroupName] resource group actually exists in the [$subscriptionName] subscription"
        echo "Creating [$resourceGroupName] resource group in the [$subscriptionName] subscription..."

        # Create the resource group
        if az group create --name "$resourceGroupName" --location "$location" 1>/dev/null; then
            echo "[$resourceGroupName] resource group successfully created in the [$subscriptionName] subscription"
        else
            echo "Failed to create [$resourceGroupName] resource group in the [$subscriptionName] subscription"
            exit
        fi
    else
        echo "[$resourceGroupName] resource group already exists in the [$subscriptionName] subscription"
    fi
}

# Validate the ARM template
validateTemplate() {
    local resourceGroupName=$1
    local template=$2
    local parameters=$3
    local arguments=$4

    # Parameters validation
    if [[ -z $resourceGroupName ]]; then
        echo "The resource group name parameter cannot be null"
    fi

    if [[ -z $template ]]; then
        echo "The template parameter cannot be null"
    fi

    if [[ -z $parameters ]]; then
        echo "The parameters parameter cannot be null"
    fi

    echo "Validating [$template] ARM template..."

    if [[ -z $arguments ]]; then
        error=$(az deployment group validate \
            --resource-group "$resourceGroupName" \
            --template-file "$template" \
            --parameters "$parameters"  2>&1 | grep 'ERROR:')
    else
        error=$(az deployment group validate \
            --resource-group "$resourceGroupName" \
            --template-file "$template" \
            --parameters "$parameters" \
            --arguments $arguments   2>&1 | grep 'ERROR:')
    fi

    if [[ -z $error ]]; then
        echo "[$template] ARM template successfully validated"
    else
        echo "Failed to validate the [$template] ARM template"
        echo "$error"
        exit 1
    fi
}

# Deploy ARM template
deployTemplate() {
    local resourceGroupName=$1
    local template=$2
    local parameters=$3
    local arguments=$4

    # Parameters validation
    if [[ -z $resourceGroupName ]]; then
        echo "The resource group name parameter cannot be null"
        exit
    fi

    if [[ -z $template ]]; then
        echo "The template parameter cannot be null"
        exit
    fi

    if [[ -z $parameters ]]; then
        echo "The parameters parameter cannot be null"
        exit
    fi

    # Deploy the ARM template
    echo "Deploying [$template] ARM template..."

    if [[ -z $arguments ]]; then
         az deployment group create \
            --resource-group $resourceGroupName \
            --template-file $template \
            --parameters $parameters 1>/dev/null
    else
         az deployment group create \
            --resource-group $resourceGroupName \
            --template-file $template \
            --parameters $parameters \
            --parameters $arguments 1>/dev/null
    fi

    if [[ $? == 0 ]]; then
        echo "[$template] ARM template successfully provisioned"
    else
        echo "Failed to provision the [$template$] ARM template"
        exit -1
    fi
}

# Create Resource Group
createResourceGroup \
    "$resourceGroupName" \
     "$location"

# Validate ARM Template
validateTemplate \
    "$resourceGroupName" \
    "$template" \
    "$parameters"

# Deploy ARM Template
deployTemplate \
    "$resourceGroupName" \
    "$template" \
    "$parameters"
```

## Create the database

You can use the `ProductsDB` T-SQL script to create the database used by the companion application. You can proceed as follows:

- VPN into the jumpbox virtual machine using Azure Bastion as shown in the picture below
- Open a browser and connect to the Azure Portal
- Open the Query Editor under the Azure SQL Database resource
- Copy and paste the code in `ProductsDB` T-SQL script into a new query
- Execute the scripts that creates the tables and some test data in the Products table used by the Web App

![Resources](images/bastion.png)

## Deploy the code of the ASP.NET Core application

This sample provides an ASP.NET Core single-page application (SPA) to test the topology. The application reads:

- Azure Cache for Redis connection string
- Azure SQL Database connection string
- Application Insights Instrumentation Key

application settings from Azure Key Vault using the following code defined in the `Program` class. For more information, see [Azure Key Vault configuration provider in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-5.0). The application uses the system-assigned managed identity of the App Service to access secrets from Azure Key Vault. The ARM template creates Key Vault, the secrets used application settings by the ASP.NET Core aaplication, and the access policies to grant permissions on secrets to the system-assigned managed identity. For more information, see [How to use managed identities for App Service and Azure Functions](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=dotnet).

### Program.cs

```csharp
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Products.Properties;

namespace Products
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var builtConfig = config.Build();
                    var keyVaultUri = builtConfig[Resources.KeyVaultUri];
                    if (string.IsNullOrEmpty(keyVaultUri))
                    {
                        throw new Exception("KeyVaultUri parameter in the appsettings.json cannot be null or empty");
                    }
                    var secretClient = new SecretClient(
                        new Uri(keyVaultUri),
                        new DefaultAzureCredential());
                    config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                })
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
```

The application makes use of the following libraries and features:

- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) library to read, create, update, and delete string values and [sets](https://redis.io/topics/data-types) in the Azure Cache for Redis. For more information, see [Quickstart: Use Azure Cache for Redis with an ASP.NET web app](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-web-app-howto). 
- [Entity Framework 5.0](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-5.0/whatsnew) to read, create, update, and delete records from the Products table in the Azure SQL Database. For more information, see [Tutorial: Get started with EF Core in an ASP.NET MVC web app](https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/intro?view=aspnetcore-5.0).
- [Dependency injection in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) to inject services into the constructor of the class where it's used. The framework takes on the responsibility of creating an instance of the dependency and disposing of it when it's no longer needed. For more information, see the code of the `ConfigureServices` method in the `Startup` class below.

### Startup.cs

```csharp
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
```

The table below shows the code of the REST API implemented by the `ProductsController` class. This API is called via [jQuery](https://api.jquery.com/jquery.ajax/) by the client-side script running in the single-page application.

```csharp
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
```

## Deploy the code of the ASP.NET Core application

Once the Azure resources have been deployed to Azure (which can take about 10-12 minutes), you need to deploy the ASP.NET Core web application contained in the `src` folder to the newly created Azure App Service. Azure App Service provides an Advanced Tool (Kudu) site that you can use to manage web app deployments. This site is accessed from a URL like: <WEB_APP_NAME>.scm.azurewebsites.net. You can use [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/) to deploy the code of the companion ASP.NET application to the Azure App Service created by the ARM template.

## Test the Application

After creating the database and deploying the Web App, you can simply navigate to the URL of your Azure App Service to check if the application is up and running, as shown in the following figure.

![Resources](images/demo.png)
