using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Otus.Teaching.Pcf.GivingToCustomer.Core.Abstractions.Gateways;
using Otus.Teaching.Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Otus.Teaching.Pcf.GivingToCustomer.DataAccess;
using Otus.Teaching.Pcf.GivingToCustomer.DataAccess.Data;
using Otus.Teaching.Pcf.GivingToCustomer.DataAccess.Repositories;
using Otus.Teaching.Pcf.GivingToCustomer.Integration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using Otus.Teaching.Pcf.GivingToCustomer.Core.Abstractions.Services;
using Otus.Teaching.Pcf.GivingToCustomer.WebHost.Models;
using Otus.Teaching.Pcf.GivingToCustomer.WebHost.Services;
using Otus.Teaching.Pcf.GivingToCustomer.WebHost.HostedServices;
using Otus.Teaching.Pcf.GivingToCustomer.WebHost.Settings;

namespace Otus.Teaching.Pcf.GivingToCustomer.WebHost
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddMvcOptions(x=> 
                x.SuppressAsyncSuffixInActionNames = false);
            services.Configure<RmqSettings>(Configuration.GetSection(RmqSettings.Section));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<INotificationGateway, NotificationGateway>();
            services.AddScoped<IDbInitializer, EfDbInitializer>();
            services.AddDbContext<DataContext>(x =>
            {
                //x.UseSqlite("Filename=PromocodeFactoryGivingToCustomerDb.sqlite");
                x.UseNpgsql(Configuration.GetConnectionString("PromocodeFactoryGivingToCustomerDb"));
                //x.UseSnakeCaseNamingConvention();
                x.UseLazyLoadingProxies();
            });
            services.AddScoped<IPromocodeService<GivePromoCodeRequest>, PromocodeService>();
            services.AddHostedService<PromocodeConsumerService>();

            services.AddOpenApiDocument(options =>
            {
                options.Title = "PromoCode Factory Giving To Customer API Doc";
                options.Version = "1.0";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseOpenApi();
            app.UseSwaggerUi(x =>
            {
                x.DocExpansion = "list";
            });
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            dbInitializer.InitializeDb();
        }
    }
}