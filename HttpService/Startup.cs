using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HttpService.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 구성값
            services.Configure<AppOptions>(options =>
            {
                string procedures = Configuration.GetValue<string>("App:allow_proc_list") ?? String.Empty;
                var appSection=Configuration.GetSection("App");
                
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
            services.AddSession(options => { 
            
            });

            services.AddControllers(options => {
                options.RespectBrowserAcceptHeader = false;
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
                .AddXmlSerializerFormatters();
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // 세션 사용
            app.UseSession();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default route", "{area=exists}/{controller=Default}/{action=Index}");
            });
        }
    }
}
