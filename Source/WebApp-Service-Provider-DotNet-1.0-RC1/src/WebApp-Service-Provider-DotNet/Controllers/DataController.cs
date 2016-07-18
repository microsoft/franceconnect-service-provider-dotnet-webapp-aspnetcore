//
// The MIT License (MIT)
// Copyright (c) 2016 Microsoft France
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// You may obtain a copy of the License at https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using WebApp_Service_Provider_DotNet.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.OptionsModel;
using WebApp_Service_Provider_DotNet.ViewModels.Data;
using Newtonsoft.Json;
using System.Net;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

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
        public IActionResult GetResource(string scheme)
        {
            var redirectUrl = Url.Action(nameof(GetResourceCallback), new { Scheme = scheme });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(scheme, redirectUrl);
            return new ChallengeResult(scheme, properties);
        }

        //
        // GET: /Data/GetResourceCallback
        [HttpGet]
        public async Task<IActionResult> GetResourceCallback(string scheme)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                Response.Redirect(Url.Action(nameof(HomeController.Index), "Home"));
            }
            var accessTokenClaim = info.ExternalPrincipal.Claims.FirstOrDefault(claim => claim.Type == "access_token");
            if (accessTokenClaim == null)
            {
                throw new NotSupportedException("Access token can not be null");
            }
            string accessToken = accessTokenClaim.Value;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync(GetResourceUrl(scheme));
            if (response.IsSuccessStatusCode)
            {
                var resource = await response.Content.ReadAsStringAsync();
                return View(ConvertResource(resource, scheme));
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                ViewData["Message"] = "La ressource demandée n'a pas été trouvée.";
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

        #region Helpers

        private BaseResourceViewModel ConvertResource(string json, string scheme)
        {
            switch (scheme)
            {
                case "DGFIP":
                    return JsonConvert.DeserializeObject<DgfipResourceViewModel>(json);
                case "Custom":
                    return JsonConvert.DeserializeObject<CustomResourceViewModel>(json);
                default:
                    throw new NotImplementedException();
            }
        }

        private string GetResourceUrl(string scheme)
        {
            DataProvider provider = _config.DataProviders.FirstOrDefault(dp => dp.Name == scheme);
            if (provider != null)
            {
                return provider.Endpoint;
            }
            else
            {
                throw new InvalidOperationException(string.Format("Unable to find \"{0}\" data provider", scheme));
            }
        }

        #endregion
    }
}
