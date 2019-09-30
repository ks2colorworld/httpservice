using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HttpService.Controllers;
using HttpService.Lib;
using HttpService.Middlewares;
using HttpService.Models;
using HttpService.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
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
            // 의존성 주입
            services.AddSingleton<IHttpContextAccessor>(_ => new HttpContextAccessor());
            services.AddSingleton<IWebHostEnvironment>(_ => HostEnvironment);
            services.AddSingleton<IConfiguration>(_ => Configuration);

            services.AddTransient<RequestDataParser>();
            services.AddTransient<XMLCommonUtil>();
            services.AddTransient<FileCommonUtil>();
            services.AddTransient<ExcelDownload>();
            services.AddTransient<SendMobileMSGCommon>();
            services.AddTransient<SendEmail>();
            services.AddTransient<Gmail>();
            services.AddTransient<UploadAndPostTwitPic>();

            services.AddTransient<DefaultController>();

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


            // session 사용
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddDistributedMemoryCache();

            services.AddControllers(options =>
            {
                //options.EnableEndpointRouting = true;

                //options.AllowEmptyInputInBodyModelBinding = true;
                //options.RespectBrowserAcceptHeader = false;
            })
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

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers();

                //endpoints.MapControllerRoute(
                //      name: "areaRoute",
                //      pattern: "{area:exists}/{controller}/{action}",
                //      defaults: new { action = "Index" });

                //endpoints.MapControllerRoute(
                //        name: "default",
                //        pattern: "{controller}/{action}/{id?}",
                //        defaults: new { controller = "Default", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "{controller}/{id?}",
                     defaults: new { controller = "Default" });

            });


            app.UseDefault();

            //app.Map("/default", (a) => {
            //    a.Use(async  (context, next) => {
            //        //if (context.Request.Path == "/" || context.Request.Path.Value.ToLower() == "/default")
            //        //{
            //            if (context.Request.Method.ToLower() == "post")
            //            {
            //                var data = String.Empty;
            //                using (var reader = new StreamReader(context.Request.Body))
            //                {
            //                    data = await reader.ReadToEndAsync();
            //                    reader.Close();
            //                }

            //                RequestModel requestModel = new RequestModel();
            //                if (!String.IsNullOrWhiteSpace(data))
            //                {
            //                    requestModel = System.Text.Json.JsonSerializer.Deserialize<RequestModel>(data, new System.Text.Json.JsonSerializerOptions
            //                    {
            //                        AllowTrailingCommas = true,
            //                        IgnoreNullValues = true,
            //                        PropertyNameCaseInsensitive = true,

            //                    });
            //                }

            //                var controller = context.RequestServices.GetService<DefaultController>();

            //                controller.Post(requestModel);

            //                return;
            //            }
            //        //}
            //    });
            //});

            //app.Run((ctx) => {
            //    var endpoint=ctx.GetEndpoint();
            //    return Task.CompletedTask;                
            //});
        }
    }
}
