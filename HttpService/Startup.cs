using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HttpService.Lib;
using HttpService.Middlewares;
using HttpService.Options;
using HttpService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpService
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
             .ConfigureApiBehaviorOptions(options =>
             {
                 // 바인딩 소스 유추 사용안함
                 //options.SuppressInferBindingSourcesForParameters = true;
             })
             .AddJsonOptions(options =>
             {
                 options.JsonSerializerOptions.PropertyNamingPolicy = null;
                 options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                 options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                 options.JsonSerializerOptions.IgnoreNullValues = true;
                 options.JsonSerializerOptions.AllowTrailingCommas = true;
                 options.JsonSerializerOptions.WriteIndented = true;
             })
             .AddXmlDataContractSerializerFormatters()
             .AddXmlSerializerFormatters()
             ;

            // 의존성 주입
            services.AddSingleton<IHttpContextAccessor>(_ => new HttpContextAccessor());
            services.AddSingleton<IWebHostEnvironment>(_ => HostEnvironment);
            services.AddSingleton<IConfiguration>(_ => Configuration);

            services.AddTransient<Gmail>();

            // 기본 서비스 구현 
            services.AddDefaultServices();

            // 구성값
            services.Configure<AppOptions>(options =>
            {
                string procedures = Configuration.GetValue<string>("App:allow_proc_list") ?? String.Empty;
                var appSection = Configuration.GetSection("App");

                options.DbConnectionConfig = Configuration.GetValue<string>("App:dbconnectionconfig");
                options.DbSchema = Configuration.GetValue<string>("App:dbschema");
                options.Test = Configuration.GetValue<string>("App:test");
                options.Etc = Configuration.GetValue<string>("App:etc");
                options.Mms = Configuration.GetValue<string>("App:mms");
                options.MmsPublicKey = Configuration.GetValue<string>("App:mms_public_key");
                // TODO 목록이 비어있을 때 문제가 없는지 확인
                options.AllowProcedureList = procedures.Trim()
                    .Split(",", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                options.Properties = appSection.AsEnumerable().ToDictionary(item => item.Key, item => item.Value);
            });

            services.Configure<EmailOptions>(options => {
                options.SenderEmailAddress = Configuration.GetValue<string>("Email:SenderEmailAddress");
                options.SenderName = Configuration.GetValue<string>("Email:SenderName");
            });

            services.Configure<GmailOptions>(Configuration.GetSection("Gmail"));
            services.Configure<SendGridOptions>(Configuration.GetSection("SendGrid"));

            // session 사용
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddDistributedMemoryCache();

            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            // 세션 사용
            app.UseSession();

            app.UseDefaultMiddlewares();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/health");
            });

            app.Map("/routes", (a) =>
            {
                a.Run(async context =>
                {
                    var endpointFeature = context.Features.Get<IEndpointFeature>();
                    var ep = endpointFeature.Endpoint;
                    var endpoint = context.GetEndpoint();
                    var routes = context.GetRouteData().Routers.OfType<RouteCollection>().First();

                    await context.Response.WriteAsync("Total number of routes: " + routes.Count.ToString() + Environment.NewLine);
                    for (int i = 0; i < routes.Count; i++)
                    {
                        await context.Response.WriteAsync(routes[i].ToString() + Environment.NewLine);
                    }

                    await context.Response.WriteAsync(endpoint?.DisplayName ?? "endpoint not fount.");
                });
            });
        }
    }
}
