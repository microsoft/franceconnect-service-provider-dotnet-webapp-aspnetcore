﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations;

namespace WebApp_Service_Provider_DotNet.ViewModels.Account
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Adresse e-mail")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Sexe")]
        public string Gender { get; set; }

        [Required]
        [Display(Name = "Date de naissance")]
        public DateTimeOffset Birthdate { get; set; }

        [Display(Name = "Nom d'usage")]
        public string PreferredUsername { get; set; }
        
        [Required]
        [Display(Name = "Prénom")]
        public string GivenName { get; set; }

        [Required]
        [Display(Name = "Nom")]
        public string FamilyName { get; set; }
    }
}
