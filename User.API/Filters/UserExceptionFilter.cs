using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace User.API.Filters
{
    public class UserExceptionFilter : IExceptionFilter
    {
        private ILogger<UserExceptionFilter> _logger;
        private readonly IHostingEnvironment _env;

        public UserExceptionFilter(ILogger<UserExceptionFilter> logger, IHostingEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            JsonErrorResponse response = new JsonErrorResponse();
          
            response.Message = context.Exception.GetType() == typeof(UserException) ? context.Exception.Message : "发生了未知的服务器内部错误";
            if (_env.IsDevelopment())
            {
                response.DeveloperMessage = context.Exception.StackTrace;
            }
            context.Result = new BadRequestObjectResult(response);
            _logger.LogError(context.Exception, context.Exception.Message);
        }
    }
}