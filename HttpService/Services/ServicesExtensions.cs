using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddDefaultServices(this IServiceCollection services)
        {
            services.AddTransient<IQueryManager, SqlServerQueryManager>();

            services.AddTransient<IDatabaseManager, DatabaseManager>();
            services.AddTransient<IEmailManager, EmailManager>();
            services.AddTransient<IFileManager, FileManager>();
            services.AddTransient<IHttpContextManager, HttpContextManager>();
            services.AddTransient<IMobileMessageManager, MobileMessageManager>();
            services.AddTransient<IResponsePreprocessManager, ResponsePreprocessManager>();
            services.AddTransient<ITwitPicManager, TwitPicManager>();

            return services;
        }
    }
}
