// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;

namespace WebApp_Service_Provider_DotNet.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Page de description de l'application.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Page de contact.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
