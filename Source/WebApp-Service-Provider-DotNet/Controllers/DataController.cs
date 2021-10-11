// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp_Service_Provider_DotNet.Helpers;
using WebApp_Service_Provider_DotNet.Models;
using WebApp_Service_Provider_DotNet.ViewModels.Data;

namespace WebApp_Service_Provider_DotNet.Controllers
{
    [Authorize]
    public class DataController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly FranceConnectConfiguration _config;

        public DataController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory,
            IOptions<FranceConnectConfiguration> config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _config = config.Value;
        }

        //
        // GET: /Data/
        [HttpGet]
        public IActionResult Index()
        {
            return View(_config.DataProviders);
        }

        //
        // GET: /Data/GetResource
        [HttpGet]
        public IActionResult GetResource(string provider)
        {
            var state = Guid.NewGuid().ToString("N");
            var scope = new List<string> { "openid" };
            scope.AddRange(_config.DataProviders.FirstOrDefault(dp => dp.Name == provider).Scopes);

            var json = JsonSerializer.Serialize(new ConsentCookie
            {
                Provider = provider,
                State = state
            });
            Response.Cookies.Delete("consent");
            Response.Cookies.Append("consent", Base64Encode(json), new CookieOptions { Expires = DateTimeOffset.Now.AddMinutes(15) });

            var authorizeRequest = new RequestUrl(_config.AuthorizationEndpoint);
            return Redirect(authorizeRequest.CreateAuthorizeUrl(
                clientId: _config.ClientId,
                responseType: "code",
                scope: string.Join(" ", scope),
                redirectUri: GetConsentRedirectUri(),
                state: state,
                acrValues: "eidas" + _config.EIdasLevel,
                nonce: Guid.NewGuid().ToString("N")));
        }

        //
        // GET: /Data/GetResourceCallback, or the data callback url specified in appsettings
        [HttpGet]
        public async Task<IActionResult> GetResourceCallback(string code, string state)
        {
            ConsentCookie consentCookie = null;
            string json;
            try
            {
                json = Base64Decode(Request.Cookies["consent"]);
                consentCookie = JsonSerializer.Deserialize<ConsentCookie>(json);
                Response.Cookies.Delete("consent");
            }
            catch (Exception)
            {
                throw new Exception("Unable to retrieve cookie");
            }

            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("Authorization code cannot be null");
            }
            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentNullException("State cannot be null");
            }
            if (state != consentCookie.State)
            {
                throw new ArgumentException("Invalid state");
            }

            var tokenClient = new HttpClient();
            var tokenResponse = await tokenClient.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
            {
                Address = _config.TokenEndpoint,
                ClientId = _config.ClientId,
                ClientSecret = _config.ClientSecret,
                Method = HttpMethod.Post,
                Code = code,
                RedirectUri = GetConsentRedirectUri()
            });
            if (tokenResponse.IsError || string.IsNullOrEmpty(tokenResponse.AccessToken) || string.IsNullOrEmpty(tokenResponse.IdentityToken))
            {
                throw new Exception("Unable to retrieve access token");
            }

            JwtSecurityToken IdToken = Validation.ReadAndValidateToken(tokenResponse.IdentityToken, new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.ClientSecret)));

            if (IdToken == null)
            {
                throw new Exception("Invalid IdToken");
            }

            if (!Validation.IsEIdasLevelMet(IdToken.Payload.Acr, _config.EIdasLevel))
            {
                throw new Exception("EIdasLevel not met");
            }

            UserLoginInfo FCUserAccount = await GetUserFCAccount();
            if (FCUserAccount != null & FCUserAccount.ProviderKey != Hashing.HashString(IdToken.Payload.Sub))
            {
                throw new Exception("Unexpected sub");
            }

            consentCookie.State = null;
            consentCookie.Token = tokenResponse.AccessToken;
            json = JsonSerializer.Serialize(consentCookie);
            Response.Cookies.Append("consent", Base64Encode(json), new CookieOptions { Expires = DateTimeOffset.Now.AddMinutes(15) });

            return RedirectToAction(nameof(Resource));
        }

        private async Task<UserLoginInfo> GetUserFCAccount()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            IList<UserLoginInfo> userLogins = await _userManager.GetLoginsAsync(user);
            UserLoginInfo FCUserAccount = userLogins.FirstOrDefault(auth => auth.LoginProvider == FranceConnectConfiguration.ProviderScheme);
            return FCUserAccount;
        }

        //
        // GET: /Data/Resource
        [HttpGet]
        public async Task<IActionResult> Resource()
        {
            ConsentCookie consentCookie;
            try
            {
                var json = Base64Decode(Request.Cookies["consent"]);
                consentCookie = JsonSerializer.Deserialize<ConsentCookie>(json);
            }
            catch (Exception)
            {
                ViewData["Message"] = "Impossible d'obtenir l'autorisation d'accès aux ressources, veuillez réessayer.";
                return View();
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", consentCookie.Token);
            var response = await client.GetAsync(GetResourceUrl(consentCookie.Provider));
            if (response.IsSuccessStatusCode)
            {
                var resource = await response.Content.ReadAsStringAsync();
                BaseResourceViewModel resourceViewModel = null;
                try
                {
                    resourceViewModel = ConvertResource(resource, consentCookie.Provider);
                }
                catch (JsonException)
                {
                    ViewData["Message"] = "Les données n'ont pas pu être désérialisées.";
                }
                return View(resourceViewModel);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                ViewData["Message"] = "La ressource demandée n'a pas été trouvée.";
                if (consentCookie.Provider=="Custom"){
                    UriBuilder addDataUri = new UriBuilder(GetResourceUrl(consentCookie.Provider))
                    {
                        Path = "/Account/Register"
                    };
                    ViewData["Register-url"] = addDataUri.Uri;
                }
                return View();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ViewData["Message"] = "Vous n'êtes pas autorisé à accéder cette ressource.";
                return View();
            }
            else
            {
                ViewData["Message"] = "Impossible de récupérer les données auprès du fournisseur choisi.";
                return View();
            }
        }

        private class ConsentCookie
        {
            public string Provider { get; set; }
            public string State { get; set; }
            public string Token { get; set; }
        }

        #region Helpers

        private BaseResourceViewModel ConvertResource(string json, string scheme)
        {
            return scheme switch
            {
                "DGFIP" => JsonSerializer.Deserialize<DgfipResourceViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                "Custom" => JsonSerializer.Deserialize<CustomResourceViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                _ => throw new NotImplementedException(),
            };
        }

        private string GetResourceUrl(string providerName)
        {
            DataProvider provider = _config.DataProviders.FirstOrDefault(dp => dp.Name == providerName);
            if (provider != null)
            {
                return provider.Endpoint;
            }
            else
            {
                throw new InvalidOperationException(string.Format("Unable to find \"{0}\" data provider", providerName));
            }
        }

        private string GetConsentRedirectUri()
        {
            return _config.DataCallbackPath != null
                ? Request.Scheme + "://" + Request.Host + _config.DataCallbackPath
                : Request.Scheme + "://" + Request.Host + Url.Action(nameof(GetResourceCallback));
        }

        private string Base64Encode(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        private string Base64Decode(string base64EncodedText)
        {
            var bytes = Convert.FromBase64String(base64EncodedText);
            return Encoding.UTF8.GetString(bytes);
        }

        #endregion
    }
}