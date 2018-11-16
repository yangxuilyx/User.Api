using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using User.API.Models;

namespace User.API.Data
{
    public class UserContextSeed
    {
        private ILogger<UserContextSeed> _logger;

        public UserContextSeed(ILogger<UserContextSeed> logger)
        {
            _logger = logger;
        }

        public static async Task SeedAsync(IApplicationBuilder applicationBuilder, ILoggerFactory loggerFactory,
            int? retry = 0)
        {
            var retryForAvaiability = retry.Value;

            try
            {
                using (var scope = applicationBuilder.ApplicationServices.CreateScope())
                {
                    var context = (UserContext)scope.ServiceProvider.GetService(typeof(UserContext));
                    var logger =
                        (ILogger<UserContextSeed>)scope.ServiceProvider.GetService<ILogger<UserContextSeed>>();

                    logger.LogDebug("Begin UserContextSeed SeedAsync");

                    context.Database.Migrate();

                    if (!context.AppUsers.Any())
                    {
                        context.AppUsers.Add(new AppUser() { Name = "jesse" });
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                if (retryForAvaiability < 10)
                {
                    retryForAvaiability++;

                    var logger = loggerFactory.CreateLogger<ILogger<UserContextSeed>>();

                    logger.LogDebug(e.Message);

                    await SeedAsync(applicationBuilder, loggerFactory, retryForAvaiability);
                }
            }
        }
    }
}