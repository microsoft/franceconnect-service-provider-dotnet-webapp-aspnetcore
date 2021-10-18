// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace WebApp_Service_Provider_DotNet
{
    public class FranceConnectConfiguration
    {
        public const string ProviderScheme = "oidc_FranceConnect";
        public const string ProviderDisplayName = "FranceConnect";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutCallbackPath { get; set; }
        public string DataCallbackPath { get; set; }
        public string Issuer { get; set; }
        public string AuthorizationEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string UserInfoEndpoint { get; set; }
        public string EndSessionEndpoint { get; set; }
        private int _EIdasLevel;
        public int EIdasLevel
        {
            get => _EIdasLevel;

            //Valid if between 1 & 3, invalid (set to 1 instead) otherwise
            set => _EIdasLevel = value > 0 && value < 4 ? value : 1;
        }
        public List<string> Scopes { get; set; }
        public List<DataProvider> DataProviders { get; set; }
    }

    public class DataProvider
    {
        public string Name { get; set; }
        public List<string> Scopes { get; set; }
        public string Endpoint { get; set; }
    }
}
