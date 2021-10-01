// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

namespace WebApp_Service_Provider_DotNet.Helpers
{
    public static class Validation
    {
        /// <summary>
        /// Parse an acr_values string and checks if it containsan eidas claim above the required one
        /// </summary>
        public static bool IsEIdasLevelMet(string acrValues, int minimumEIdasLevel)
        {
            if (acrValues != null)
            {
                Regex myRegex = new Regex(@"eidas(\d)");
                Match match = myRegex.Match(acrValues);
                if (match.Success)
                {
                    int eIdasLevel = int.Parse(match.Groups[1].Value);
                    if (eIdasLevel >= minimumEIdasLevel)
                        return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Validate the token signature and reads it if valid
        /// </summary>
        /// <param name="token">the JWT to read</param>
        /// <param name="securityKey">the security key to use for signature validation</param>
        /// <returns>The decoded token if the signature was valid, null otherwise.</returns>
        static public JwtSecurityToken ReadAndValidateToken(string token, SecurityKey securityKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateActor = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey
            };

            SecurityToken validatedToken;
            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (Exception)
            {
                return null;
            }
            return (JwtSecurityToken)validatedToken;
        }
    }
}
