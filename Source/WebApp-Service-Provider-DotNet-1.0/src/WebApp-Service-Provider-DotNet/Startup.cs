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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using WebApp_Service_Provider_DotNet.Models;
using WebApp_Service_Provider_DotNet.Services;

namespace WebApp_Service_Provider_DotNet
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add configuration
            services.AddOptions();
            services.Configure<FranceConnectConfiguration>(Configuration.GetSection("FranceConnect"));

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<FranceConnectConfiguration> opts)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture("fr-FR") });

            app.UseStaticFiles();

            app.UseIdentity();

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715
            var franceConnectOptions = new OpenIdConnectOptions();
            franceConnectOptions.AuthenticationScheme = Scheme.FranceConnect;
            franceConnectOptions.DisplayName = "FranceConnect";
            franceConnectOptions.ClientId = opts.Value.ClientId;
            franceConnectOptions.ClientSecret = opts.Value.ClientSecret;
            franceConnectOptions.Authority = opts.Value.Issuer;
            franceConnectOptions.ResponseType = OpenIdConnectResponseType.Code;
            franceConnectOptions.Scope.Clear();
            franceConnectOptions.Scope.Add("openid");
            franceConnectOptions.Scope.Add("profile");
            franceConnectOptions.Scope.Add("birth");
            franceConnectOptions.Scope.Add("email");
            franceConnectOptions.GetClaimsFromUserInfoEndpoint = true;
            franceConnectOptions.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Value.ClientSecret));
            franceConnectOptions.Configuration = new OpenIdConnectConfiguration
            {
                Issuer = opts.Value.Issuer,
                AuthorizationEndpoint = opts.Value.AuthorizationEndpoint + "?acr_values=" + opts.Value.EIdas,
                TokenEndpoint = opts.Value.TokenEndpoint,
                UserInfoEndpoint = opts.Value.UserInfoEndpoint,
                EndSessionEndpoint = opts.Value.EndSessionEndpoint
            };
            app.UseOpenIdConnectAuthentication(franceConnectOptions);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
