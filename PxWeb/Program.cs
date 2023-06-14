using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PxWeb.Code.Api2;
using PxWeb.Config.Api2;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PxWeb.Filters.Api2;
using Px.Abstractions.Interfaces;
using PxWeb.Code.Api2.DataSource;
using PxWeb.Code.Api2.DataSource.Cnmm;
using PxWeb.Code.Api2.DataSource.PxFile;
using PxWeb.Helper.Api2;
using Microsoft.AspNetCore.Mvc;
using PxWeb.Mappers;
using Newtonsoft.Json;
using System;
using PxWeb.Code.Api2.NewtonsoftConfiguration;
using PxWeb.Middleware;
using Px.Search;
using Px.Search.Lucene;
using PxWeb.Code.Api2.Cache;
using System.Text;
using PxWeb.Code;
using PxWeb.Code.Api2.Serialization;
using PxWeb.Code.BackgroundWorker;
using PxWeb.Code.Api2.DataSelection;

using PxWeb.Logging;

namespace PxWeb
{
    public class Program
    {
        private static ILogger<Program> _logger;

        public static void Main(string[] args)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = WebApplication.CreateBuilder(args);

            // needed to load configuration from appsettings.json
            builder.Services.AddOptions();

            // needed to store rate limit counters and ip rules
            builder.Services.AddMemoryCache();

            //load general configuration from appsettings.json
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            builder.Services.AddInMemoryRateLimiting();

            // configuration (resolvers, counter key builders)
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            builder.Services.AddSingleton<IPxCache, PxCache>();
            builder.Services.AddSingleton<ILinkCreator, LinkCreator>();
            builder.Services.AddSingleton<ISelectionHandler, SelectionHandler>();

            builder.Services.AddPxDataSource(builder);

            builder.Services.Configure<PxApiConfigurationOptions>(builder.Configuration.GetSection("PxApiConfiguration"));
            builder.Services.Configure<AdminProtectionConfigurationOptions>(builder.Configuration.GetSection("AdminProtection"));
            builder.Services.Configure<CacheMiddlewareConfigurationOptions>(builder.Configuration.GetSection("CacheMiddleware"));

            builder.Services.AddTransient<IPxApiConfigurationService, PxApiConfigurationService>();
            builder.Services.AddTransient<IAdminProtectionConfigurationService, AdminProtectionConfigurationService>();
            builder.Services.AddTransient<ICacheMiddlewareConfigurationService, CacheMiddlewareConfigurationService>();
            builder.Services.AddTransient<ILanguageHelper, LanguageHelper>();
            builder.Services.AddTransient<IFolderResponseMapper, FolderResponseMapper>();
            builder.Services.AddTransient<ITableMetadataResponseMapper, TableMetadataResponseMapper>();
            builder.Services.AddTransient<ITablesResponseMapper, TablesResponseMapper>();
            builder.Services.AddTransient<ITableResponseMapper, TableResponseMapper>();
            builder.Services.AddTransient<IPxHost, PxWebHost>();
            builder.Services.AddTransient<ISerializeManager, SerializeManager>();

            builder.Services.AddHostedService<LongRunningService>();
            builder.Services.AddSingleton<BackgroundWorkerQueue>();

            builder.Services.AddPxSearchEngine(builder);

            var langList = builder.Configuration.GetSection("PxApiConfiguration:Languages")
                .AsEnumerable()
                .Where(p => p.Value != null && p.Key.ToLower().Contains("id"))
                .Select(p => p.Value)
                .ToList();

            builder.Services.AddControllers(x =>
                x.Filters.Add(new LangValidationFilter(langList))
                )
                .AddNewtonsoftJson(opts =>
            {
                //opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                opts.SerializerSettings.ContractResolver = new BaseFirstContractResolver();
                opts.SerializerSettings.Converters.Add(new StringEnumConverter
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                });
                opts.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                opts.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"; // UTC
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                // Sort endpoints
                c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");
            });


            // Add services to the container.
            _logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();


            // Handle CORS configuration from appsettings.json
            bool corsEnbled = builder.Services.ConfigurePxCORS(builder, _logger);

            builder.Services.AddLogging(builder => builder.AddCustomFormatter(options =>
                       options.CustomPrefix = ""));
            builder.Logging.AddLog4Net();


            using ILoggerFactory loggerFactory =
               LoggerFactory.Create(builder =>
                   builder.AddCustomFormatter(options =>
                       options.CustomPrefix = " ~~~~~ "));
            builder.Logging.Services.AddLogging();

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}
            app.UseHttpsRedirection();

            if (corsEnbled)
            {
                app.UseCors();
            }

            if (!app.Environment.IsDevelopment())
            {
                    app.UseAuthorization();

                app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/v2/admin"), appBuilder =>
                {
                    appBuilder.UseAdminProtectionIpWhitelist();
                    appBuilder.UseAdminProtectionKey();
                });
            }
            app.MapControllers();

            if (!app.Environment.IsDevelopment())
            {
                app.UseIpRateLimiting();
            }

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/v2/admin"), appBuilder =>
            {
                appBuilder.UseCacheMiddleware();
            });
            Console.WriteLine("{\r\n  \"severity\":\"ERROR\",\r\n  \"message\":\"There was an error in the application.\",\r\n  \"httpRequest\":{\r\n    \"requestMethod\":\"GET\"\r\n  },\r\n  \"times\":\"2020-10-12T07:20:50.52Z\",\r\n  \"logging.googleapis.com/insertId\":\"42\",\r\n  \"logging.googleapis.com/labels\":{\r\n    \"user_label_1\":\"value_1\",\r\n    \"user_label_2\":\"value_2\"\r\n  },\r\n  \"logging.googleapis.com/operation\":{\r\n    \"id\":\"get_data\",\r\n    \"producer\":\"github.com/MyProject/MyApplication\",\r\n     \"first\":\"true\"\r\n  },\r\n  \"logging.googleapis.com/sourceLocation\":{\r\n    \"file\":\"get_data.py\",\r\n    \"line\":\"142\",\r\n    \"function\":\"getData\"\r\n  },\r\n  \"logging.googleapis.com/spanId\":\"000000000000004a\",\r\n  \"logging.googleapis.com/trace\":\"projects/my-projectid/traces/06796866738c859f2f19b7cfb3214824\",\r\n  \"logging.googleapis.com/trace_sampled\":false}");
            app.UseHttpLogging();
            app.Logger.LogInformation("*********** app.Run() :");
            app.Run();
        }
    }
}