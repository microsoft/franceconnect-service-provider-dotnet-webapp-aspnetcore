// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WebApp_Service_Provider_DotNet.Helpers;

namespace WebApp_Service_Provider_DotNet.Services
{
    class FCSignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        public FCSignInManager(UserManager<TUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<TUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<TUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<TUser> confirmation)
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