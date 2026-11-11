using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ExonymsAPI.Service;
using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Client.TransliterationAPI;
using ExonymsAPI.Configuration;
using NuciAPI.Middleware.ExceptionHandling;
using Moq;

namespace ExonymsAPI.IntegrationTests
{
    public sealed class ExonymsApiFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
            => base.CreateHost(builder);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the Startup registration
                var startupDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStartup) || d.ImplementationType == typeof(Startup));
                if (startupDescriptor != null)
                {
                    services.Remove(startupDescriptor);
                }

                // Add required services - replicate what Startup.ConfigureServices does
                services
                    .AddControllers()
                    .AddApplicationPart(typeof(ExonymsAPI.API.Controllers.ExonymsController).Assembly);
                services.AddAuthorization();
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                        policy
                            .WithOrigins(
                                "https://hmlendea.github.io",
                                "http://localhost:5000",
                                "https://localhost:5001",
                                "http://localhost:7000",
                                "https://localhost:7001",
                                "http://localhost:8080",
                                "http://localhost:8081")
                            .AllowAnyHeader()
                            .AllowAnyMethod());
                });

                // Register all configuration settings manually (bypass AddConfigurations to avoid binding issues)
                SecuritySettings securitySettings = new()
                {
                    HmacSigningKey = "TestSigningKey123456789012345678901234"
                };
                services.AddSingleton(securitySettings);
                services.AddSingleton(new ExonymsAPI.Configuration.TransliterationSettings
                {
                    TransliterationApiBaseUrl = "http://localhost"
                });
                services.AddSingleton(new NuciLog.Configuration.NuciLoggerSettings());

                // Add custom services but remove the external service registrations
                services.AddCustomServices();

                // Re-register mocks after AddCustomServices to override the real implementations
                var geoNamesDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IGeoNamesGatherer));
                if (geoNamesDescriptor != null)
                {
                    services.Remove(geoNamesDescriptor);
                }

                var wikiDataDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IWikiDataGatherer));
                if (wikiDataDescriptor != null)
                {
                    services.Remove(wikiDataDescriptor);
                }

                var transliterationDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITransliterationApiClient));
                if (transliterationDescriptor != null)
                {
                    services.Remove(transliterationDescriptor);
                }

                // Add mock services
                var mockGeoNamesGatherer = new Mock<IGeoNamesGatherer>();
                mockGeoNamesGatherer.Setup(x => x.Gather("310350"))
                    .ReturnsAsync(new ExonymsAPI.Service.Models.Location("GeoNamesDefault")
                    {
                        Names = new System.Collections.Generic.Dictionary<string, ExonymsAPI.Service.Models.Name>
                        {
                            { "de", new ExonymsAPI.Service.Models.Name("GermanName") { Value = "GermanValue" } }
                        }
                    });
                mockGeoNamesGatherer.Setup(x => x.Gather("invalid"))
                    .ReturnsAsync(new ExonymsAPI.Service.Models.Location(""));
                services.AddSingleton(mockGeoNamesGatherer.Object);

                var mockWikiDataGatherer = new Mock<IWikiDataGatherer>();
                mockWikiDataGatherer.Setup(x => x.Gather("Q310350"))
                    .ReturnsAsync(new ExonymsAPI.Service.Models.Location("WikiDataDefault")
                    {
                        Names = new System.Collections.Generic.Dictionary<string, ExonymsAPI.Service.Models.Name>
                        {
                            { "fr", new ExonymsAPI.Service.Models.Name("FrenchName") { Value = "FrenchValue" } }
                        }
                    });
                services.AddSingleton(mockWikiDataGatherer.Object);

                var mockTransliterationClient = new Mock<ITransliterationApiClient>();
                mockTransliterationClient.Setup(x => x.Transliterate(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync("TransliteratedText");
                services.AddTransient<ITransliterationApiClient>(_ => mockTransliterationClient.Object);
            });

            builder.Configure(app =>
            {
                app.UseNuciApiExceptionHandling();

                if (app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
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
            });

            base.ConfigureWebHost(builder);
        }
    }
}