using System.Web.Http;

namespace ProductStore
{
    public class ResolverConfig
    {
        public static void ConfigureApi(HttpConfiguration config)
        {
            config.DependencyResolver = new SimpleContainer();
        }
    }
}