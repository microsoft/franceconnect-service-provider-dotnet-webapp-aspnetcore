// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Identity;
using System;

namespace WebApp_Service_Provider_DotNet.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Gender { get; set; }
        public DateTimeOffset? Birthdate { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string PreferredName { get; set; }
    }
}
