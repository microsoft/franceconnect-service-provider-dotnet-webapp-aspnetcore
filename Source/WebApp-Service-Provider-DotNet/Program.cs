﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;
using WebApp_Service_Provider_DotNet;
using WebApp_Service_Provider_DotNet.Models;
using WebApp_Service_Provider_DotNet.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

if (string.IsNullOrEmpty(builder.Configuration["FranceConnect:ClientSecret"]))
{
    throw new InvalidOperationException("FC Client Secret not found. It must be added to the configuration, through User Secrets for example.");
    // User-Secrets documentation : https://docs.asp.net/en/latest/security/app-secrets.html
}

// Add configuration
builder.Services.AddOptions();

// Since chromium updates to SameSite cookie policies, this must be used for the authentication cookies to avoid a Correlation error without HTTPS
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

IConfiguration franceConnectConfig = builder.Configuration.GetSection("FranceConnect");
builder.Services.Configure<FranceConnectConfiguration>(franceConnectConfig);

// Add framework services.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        // Suggested for development environments, as the database is thus hosted with the web app instead of a separate server.
        options.UseInMemoryDatabase("InMemory");
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager<FCSignInManager>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

builder.Services.AddAuthentication()
    .AddOpenIdConnect(FranceConnectConfiguration.ProviderScheme, FranceConnectConfiguration.ProviderDisplayName, oidc_options =>
    {
        var fcConfig = franceConnectConfig.Get<FranceConnectConfiguration>();

        // FC refuses unknown parameters in the requests, so the two following options are needed 
        oidc_options.DisableTelemetry = true; // This is false by default on .NET Core 3.1, and sends additional parameters such as "x-client-ver" in the requests to FC.
        oidc_options.UsePkce = false; // This is true by default on .NET Core 3.1, and enables the PKCE mechanism which is not supported by FC.

        // FC has restrictions in the nonce (max 128 alphanumeric characters) and errors out in the logout flow otherwise. We use this option so that the nonce does not contain a dot.
        oidc_options.ProtocolValidator.RequireTimeStampInNonce = false;

        oidc_options.SaveTokens = true; // This is needed to keep the id_token obtained for authentication : we have to send it back to FC to logout.

        oidc_options.ClientId = fcConfig.ClientId;
        oidc_options.ClientSecret = fcConfig.ClientSecret;
        oidc_options.CallbackPath = fcConfig.CallbackPath;
        oidc_options.SignedOutCallbackPath = fcConfig.SignedOutCallbackPath;
        oidc_options.Authority = fcConfig.Issuer;
        oidc_options.ResponseType = OpenIdConnectResponseType.Code;
        oidc_options.Scope.Clear();
        oidc_options.Scope.Add("openid");
        foreach (string scope in fcConfig.Scopes)
        {
            oidc_options.Scope.Add(scope);
        }
        oidc_options.GetClaimsFromUserInfoEndpoint = true;
        oidc_options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(fcConfig.ClientSecret));
        oidc_options.Configuration = new OpenIdConnectConfiguration
        {
            Issuer = fcConfig.Issuer,
            AuthorizationEndpoint = fcConfig.AuthorizationEndpoint,
            TokenEndpoint = fcConfig.TokenEndpoint,
            UserInfoEndpoint = fcConfig.UserInfoEndpoint,
            EndSessionEndpoint = fcConfig.EndSessionEndpoint,
        };
        oidc_options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = (context) =>
                {
                    context.ProtocolMessage.AcrValues = "eidas" + fcConfig.EIdasLevel;
                    return Task.FromResult(0);
                }
        };
        // We specify claims to be kept, as .NET Core 2.0+ doesn't keep claims it does not expect.
        oidc_options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
        oidc_options.ClaimActions.MapUniqueJsonKey("birthcountry", "birthcountry");
        oidc_options.ClaimActions.MapUniqueJsonKey("birthdate", "birthdate");
        oidc_options.ClaimActions.MapUniqueJsonKey("birthplace", "birthplace");
        oidc_options.ClaimActions.MapUniqueJsonKey("gender", "gender");
    });

builder.Services.AddControllersWithViews();

// Add application services.
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();

var app = builder.Build();

// Configure the HTTP request pipeline

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseMigrationsEndPoint();
}

app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture("fr-FR") });

app.UseCookiePolicy();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

string dataCallbackPath = franceConnectConfig.GetValue<string>("DataCallbackPath");
if (dataCallbackPath != null)
{
    app.MapControllerRoute("data-consent-callback", dataCallbackPath, new { controller = "Data", action = "GetResourceCallback" });
}

app.Run();