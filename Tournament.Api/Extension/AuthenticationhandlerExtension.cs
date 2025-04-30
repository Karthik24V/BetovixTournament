using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Tournament.Common.Dto_s;

namespace Tournament.Api.Extension
{
    public class AuthenticationhandlerExtension : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ApiKeySettings _apiKeySettings;
        public AuthenticationhandlerExtension( IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,IConfiguration configuration, ISystemClock clock, IOptions<ApiKeySettings> apiKeyOptions) : base(options, logger, encoder, clock)
        {
            _apiKeySettings = apiKeyOptions.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            if (!Request.Headers.TryGetValue(_apiKeySettings.HeaderName, out var extractedApiKey))
                return Task.FromResult(AuthenticateResult.Fail("API Key was not provided."));

            if (extractedApiKey != _apiKeySettings.ApiKey)
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key."));

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "ApiKeyUser") };
            var identity = new ClaimsIdentity(claims, nameof(AuthenticationhandlerExtension));
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "ApiKeyScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
