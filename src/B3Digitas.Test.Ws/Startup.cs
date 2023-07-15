using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using System.Net.WebSockets;

namespace B3Digitas.Test.Ws
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(policyBuilder =>
                policyBuilder.AddDefaultPolicy(policy =>
                    policy.AllowAnyHeader().AllowAnyHeader())
            ).AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            })
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Base API", Version = "v1" });
                c.UseInlineDefinitionsForEnums();
            }).HealthChecksConfiguration(_configuration)
            .AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            }).AddApiVersioning(options => { options.ReportApiVersions = true; })
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });


            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });

            services.AddTransient<ClientWebSocket>();

            services.AddSingleton<Ws>();

            services.AddHostedService<HostServerWs>();

        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseSwagger()
                .UseSwaggerUI()
                .UseRouting()
                .UseResponseCompression()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                })
                .UseAuthorization()
                .HealthCheckConfiguration();
        }
    }
}
