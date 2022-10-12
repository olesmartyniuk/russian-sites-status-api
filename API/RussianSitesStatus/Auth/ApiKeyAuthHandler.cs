using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace RussianSitesStatus.Auth
{
    public class ApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string ApiKeyHeaderName = "ApiKey";
        private ILogger<ApiKeyAuthHandler> _logger;

        private readonly IConfiguration _configuration;
        public ApiKeyAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options
            , ILoggerFactory logger
            , UrlEncoder encoder
            , ISystemClock clock
            , IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger<ApiKeyAuthHandler>();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (!Context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKey) || _configuration["API_KEY"] != apiKey)
                {
                    return Task.FromResult(AuthenticateResult.Fail("This path is restricted to internal users only."));
                }

                var claimsIdentity = new ClaimsIdentity(Array.Empty<Claim>(), nameof(ApiKeyAuthHandler));
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The request could not be authenticated.");
                throw;
            }
        }
    }
}
