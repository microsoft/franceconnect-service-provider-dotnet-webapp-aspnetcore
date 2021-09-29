// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WebApp_Service_Provider_DotNet.Models
{
    public class PivotIdentity
    {
        public string Gender { get; set; }
        public DateTimeOffset Birthdate { get; set; }
        public string Birthcountry { get; set; }
        public string Birthplace { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string PreferredName { get; set; }
        public string Email { get; set; }
    }
}
