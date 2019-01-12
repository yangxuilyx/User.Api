using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using User.API.Controllers;
using User.API.Data;
using User.API.Models;
using Xunit;

namespace User.API.Test
{
    public class UserControllerTests
    {
        [Fact]
        public async Task Get_ReturnRightUser_WithExpectedParameters()
        {
            var controllerAndContext = GetControllerAndContext();
            var result = await controllerAndContext.controller.Get();
        }

        private (UserController controller, UserContext context) GetControllerAndContext()
        {
            var context = GetUserContext();

            var controller = new UserController(context);

            return (controller: controller, context: context);
        }

        private static UserContext GetUserContext()
        {
            DbContextOptions<UserContext> options =
                new DbContextOptionsBuilder<UserContext>().UseInMemoryDatabase("UserContext").Options;
            var userContext = new UserContext(options);
            userContext.Add(new AppUser()
            {
                Name = "jesse",
                Id = 1
            });

            userContext.SaveChanges();
            return userContext;
        }
    }
}