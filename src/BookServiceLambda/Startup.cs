using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using BookService.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookService
{
    public class Startup
    {
        public IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddAWSService<IAmazonDynamoDB>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddLogging();

            return services.BuildServiceProvider();
        }
    }
}
