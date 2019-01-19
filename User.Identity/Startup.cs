using System.Net.Http;
using DnsClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Resilience;
using Microsoft.Extensions.Options;
using User.Identity.Authentication;
using User.Identity.Infrastructure;
using User.Identity.Dtos;
using User.Identity.Services;

namespace User.Identity
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
            services.AddIdentityServer().AddDeveloperSigningCredential()
                .AddExtensionGrantValidator<SmsAuthCodeValidator>()
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients());

            services.AddSingleton<HttpClient>(new HttpClient());

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));

            services.AddSingleton<IDnsQuery>(p =>
            {
                var serviceOptions = p.GetService<IOptions<ServiceDiscoveryOptions>>();

                return new LookupClient(serviceOptions.Value.Consul.DnsEndpoint.ToIPEndPoint());
            });

            services.AddScoped<IAuthService, TestAuthService>();
            services.AddScoped<IUserService, UserService>();

            services.AddSingleton<ResilienceClientFactory>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                int retryCount = 5;
                int exceptionCountAllowedBeforeBreaking = 5;

                return new ResilienceClientFactory(logger, httpContextAccessor, retryCount, exceptionCountAllowedBeforeBreaking);
            });

            // 注册全局单例IHttpClient
            services.AddSingleton<IHttpClient>(sp => sp.GetRequiredService<ResilienceClientFactory>().GetResilienceHttpClient());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
        }
    }
}
