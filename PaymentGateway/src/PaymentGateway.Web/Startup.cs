﻿using Bank.Core;
using Bank.Core.Interfaces;
using Bank.Mock;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentGateway.Core;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Web.Handler;
using Swashbuckle.AspNetCore.Swagger;

namespace PaymentGateway.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // use in memory database for the paymentDbContext
            services.AddDbContext<PaymentDbContext>(options =>
            options.UseInMemoryDatabase("paymentgateway"));

            // configure the required business objects for the payment gateway and banking service
            services.AddScoped<IRepository, PaymentRepository>();
            services.AddScoped<IPaymentManager, PaymentManager>();
            services.AddScoped<PaymentPolicy>();
            services.AddScoped<IBankService, BankService>();

            services.AddMvc()
                .AddControllersAsServices()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "PaymentGateway API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Logging configuration
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureExceptionHandler(loggerFactory.CreateLogger("ExceptionHandler"));
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentGateway API V1");
            });

            app.UseMvc();
        }
    }
}
