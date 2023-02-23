using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FrogAlert.Auth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ApiKeyFilterAttribute : Attribute, IAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-API-Key";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var submittedApiKey = GetSubmittedApiKey(context.HttpContext);

            var configuredApiKeys = GetConfiguredApiKeys(context.HttpContext);

            if (!configuredApiKeys.Contains(submittedApiKey))
            {
                context.Result = new UnauthorizedResult();
            }
        }

        private string GetSubmittedApiKey(HttpContext context)
        {
            return context.Request.Headers[ApiKeyHeaderName];
        }

        private string[] GetConfiguredApiKeys(HttpContext context)
        {
            var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

            var apiKeys = configuration.GetSection("ApiKeys").Get<string[]>();

            return apiKeys;
        }
    }
}
