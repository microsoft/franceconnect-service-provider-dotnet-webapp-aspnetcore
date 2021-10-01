// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WebApp_Service_Provider_DotNet.Helpers;
using WebApp_Service_Provider_DotNet.Models;

namespace WebApp_Service_Provider_DotNet.Services
{
    class FCSignInManager : SignInManager<ApplicationUser>
    {
        public FCSignInManager(UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<ApplicationUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<ApplicationUser> confirmation) 
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public override async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string expectedXsrf = null)
        {
            ExternalLoginInfo info = await base.GetExternalLoginInfoAsync(expectedXsrf);

            // FranceConnect indicates it would be better to hash the provided sub claim from Franceconnect when using it.
            // The sub is the ProviderKey returned from GetExternalLoginInfoAsync, which is why we override like that.
            if (info != null)
            {
                info.ProviderKey = Hashing.HashString(info.ProviderKey);
            }

            return info;
        }
    }
}