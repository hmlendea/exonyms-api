using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NuciAPI.Middleware.ExceptionHandling;
using NuciAPI.Middleware.Logging;

namespace ExonymsAPI
{
    public sealed class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        private static string[] AllowedCorsOrigins =>
        [
            "https://hmlendea.github.io",
            "http://localhost:5000",
            "https://localhost:5001",
            "http://localhost:7000",
            "https://localhost:7001",
            "http://localhost:8080",
            "http://localhost:8081"
        ];

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy
                        .WithOrigins(AllowedCorsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            services
                .AddConfigurations(Configuration)
                .AddCustomServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseNuciApiRequestLogging();
            app.UseNuciApiExceptionHandling();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
