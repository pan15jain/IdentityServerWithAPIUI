using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IdentityServer.Factories
{
    public class TrimarkUserClaimsPrincipleFactory:UserClaimsPrincipalFactory<TrimarkUser>
    {
        public TrimarkUserClaimsPrincipleFactory(UserManager<TrimarkUser> userManager,IOptions<IdentityOptions> options):base(userManager,options)  
        {

        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync( TrimarkUser user)
        {
            var claimsIdentity=await base.GenerateClaimsAsync(user);
            //Add the custom claim
            return claimsIdentity;

        }
    }
}
