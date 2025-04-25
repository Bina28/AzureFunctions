using FunctionAppAzure;
using FunctionAppAzure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var host = new HostBuilder()
	.ConfigureFunctionsWebApplication()
	.ConfigureAppConfiguration(config =>
	{
		// Load environment variables (Azure will use Application Settings)
		config.AddEnvironmentVariables();
	})
	.ConfigureServices((context, services) =>
	{
		services.AddLogging();
		services.AddOptions();
		services.AddSingleton<IEmailService, EmailService>();
		services.AddSingleton(sp =>
			new CosmosClient(Environment.GetEnvironmentVariable("CosmosDBConnection")));
	})
	.Build();

host.Run();