using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HappyCode.NetCoreBoilerplate.Api.Infrastructure.Configurations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using HappyCode.NetCoreBoilerplate.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HappyCode.NetCoreBoilerplate.Api.BackgroundServices;
using HappyCode.NetCoreBoilerplate.Api.Infrastructure.Filters;
using Microsoft.Extensions.Hosting;

namespace HappyCode.NetCoreBoilerplate.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                    options.Filters.Add(typeof(ValidateModelStateFilter));
                })
                .AddApiExplorer()
                .AddDataAnnotations()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddHttpContextAccessor();

            services.AddDbContext<EmployeesContext>(options => options.UseMySQL(_configuration.GetConnectionString("MySqlDb")), ServiceLifetime.Transient);
            services.AddDbContext<CarsContext>(options => options.UseSqlServer(_configuration.GetConnectionString("MsSqlDb")), ServiceLifetime.Transient);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Simple Api", Version = "v1" });      //needs to be fixed after official release of Swashbuckle.AspNetCore 5.x
                c.DescribeAllParametersInCamelCase();
                c.OrderActionsBy(x => x.RelativePath);
            });

            services.Configure<PingWebsiteConfiguration>(_configuration.GetSection("PingWebsite"));
            services.AddHostedService<PingWebsiteBackgroundService>();
            services.AddHttpClient(nameof(PingWebsiteBackgroundService));

            services.AddHealthChecks();
        }

        public virtual void ConfigureContainer(ContainerBuilder builder)
        {
            ContainerConfigurator.RegisterModules(builder);
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/healthcheck");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple Api V1");
                c.DocExpansion(DocExpansion.None);
            });
        }
    }
}
