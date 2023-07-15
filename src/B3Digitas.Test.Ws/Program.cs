//using Microsoft.AspNetCore.Hosting;
//using System.Globalization;

//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.Run();



using B3Digitas.Test.Ws;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using System.Globalization;

public class Program
{
    public static Task Main(string[] args)
    {
        return CreateHostBuilder(args).UseDefaultServiceProvider(options => options.ValidateScopes = false)
            .Build().RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
            .UseSerilog((context, configuration) =>
            {
                configuration
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.Debug()
                    .WriteTo.Console(new JsonFormatter())
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions()
                    {
                        AutoRegisterTemplate = true,
                        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                        IndexFormat = "webapi-{0:yyyy.MM}"
                    })
                    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!)
                    .ReadFrom.Configuration(context.Configuration);
            });
    }
}