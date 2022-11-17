using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorUI.Handlers
{
    public class ApiAuthorizationMessageHandler:AuthorizationMessageHandler
    {
        public ApiAuthorizationMessageHandler(IAccessTokenProvider provider,NavigationManager navigation):base(provider,navigation)
        {
            ConfigureHandler(new List<string> { "https://localhost:7001" }, new List<string> { "http://www.trimark.com" });
        }
    }
}
