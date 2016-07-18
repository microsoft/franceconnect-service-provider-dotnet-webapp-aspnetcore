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

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApp_Service_Provider_DotNet.Models;
using WebApp_Service_Provider_DotNet.Services;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNet.Localization;

namespace WebApp_Service_Provider_DotNet
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            FcConfiguration = Configuration.GetSection("FranceConnect").Get<FranceConnectConfiguration>();
        }

        public IConfigurationRoot Configuration { get; set; }

        public FranceConnectConfiguration FcConfiguration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                        .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                             .Database.Migrate();
                    }
                }
                catch { }
            }

            app.UseRequestLocalization(new RequestCulture("fr-FR"));

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());
            
            app.UseStaticFiles();

            app.UseIdentity();

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715
            app.UseCookieAuthentication(options =>
            {
                options.AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.AutomaticAuthenticate = true;
            });
            
            app.UseOpenIdConnectAuthentication(options =>
            {
                options.AuthenticationScheme = Scheme.FranceConnect;
                options.DisplayName = "FranceConnect";
                options.ClientId = FcConfiguration.ClientId;
                options.ClientSecret = FcConfiguration.ClientSecret;
                options.Authority = FcConfiguration.Issuer;
                options.ResponseType = OpenIdConnectResponseTypes.Code;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("birth");
                options.Scope.Add("email");
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Configuration = new OpenIdConnectConfiguration
                {
                    Issuer = FcConfiguration.Issuer,
                    AuthorizationEndpoint = string.Format(FcConfiguration.AuthorizationEndpoint, FcConfiguration.EIdas),
                    TokenEndpoint = FcConfiguration.TokenEndpoint,
                    UserInfoEndpoint = FcConfiguration.UserInfoEndpoint,
                    EndSessionEndpoint = FcConfiguration.EndSessionEndpoint
                };
            });

            foreach (DataProvider resourceServer in FcConfiguration.DataProviders)
            {
                app.UseOpenIdConnectAuthentication(options =>
                {
                    options.AuthenticationScheme = resourceServer.Name;
                    options.ClientId = FcConfiguration.ClientId;
                    options.ClientSecret = FcConfiguration.ClientSecret;
                    options.Authority = FcConfiguration.Issuer;
                    options.ResponseType = OpenIdConnectResponseTypes.Code;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    foreach (string scope in resourceServer.Scopes)
                    {
                        options.Scope.Add(scope);
                    }
                    options.CallbackPath = string.Format("/consent/callback_{0}", resourceServer.Name);
                    options.Configuration = new OpenIdConnectConfiguration
                    {
                        Issuer = FcConfiguration.Issuer,
                        AuthorizationEndpoint = string.Format(FcConfiguration.AuthorizationEndpoint, FcConfiguration.EIdas),
                        TokenEndpoint = FcConfiguration.TokenEndpoint
                    };
                });
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
