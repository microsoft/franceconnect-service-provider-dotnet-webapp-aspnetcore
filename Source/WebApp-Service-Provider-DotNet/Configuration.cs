// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace WebApp_Service_Provider_DotNet
{
    public class Scheme
    {
        public const string FranceConnectDisplayName = "FranceConnect";
        public const string FranceConnect = "oidc_FranceConnect";
    }

    public class FranceConnectConfiguration
    {
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
        public string EIdas { get; set; }
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
