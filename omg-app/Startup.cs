namespace omg_app
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Models;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddApplicationInsightsTelemetry(this.Configuration);

            var serviceBusConfiguration = this.GetServiceBusConfiguration(this.Configuration);
            ServiceBusService.SetupServiceBus(serviceBusConfiguration);

            services.AddSingleton<BlobStorageInfo>(this.Configuration.GetSection(nameof(BlobStorageInfo)).Get<BlobStorageInfo>());
            services.AddTransient(typeof(BlobStorageService));

            services.AddSingleton<FaceServiceStorageInfo>(this.Configuration.GetSection(nameof(FaceServiceStorageInfo)).Get<FaceServiceStorageInfo>());
            services.AddTransient<FaceService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private ServiceBusModel GetServiceBusConfiguration(IConfiguration configuration)
        {
            var busConfiguration = configuration.GetSection("AzureServiceBus").Get<ServiceBusModel>();

            if (busConfiguration == null)
            {
                throw new ArgumentException("Invalid azure service bus configuration.");
            }

            var hasInvalidConfiguration = busConfiguration.GetType().GetProperties().Select(prop => prop.GetValue(busConfiguration).ToString()).Any(string.IsNullOrWhiteSpace);

            if (hasInvalidConfiguration)
            {
                throw new ArgumentException("Invalid azure service bus configuration properties.");
            }

            return busConfiguration;
        }
    }
}