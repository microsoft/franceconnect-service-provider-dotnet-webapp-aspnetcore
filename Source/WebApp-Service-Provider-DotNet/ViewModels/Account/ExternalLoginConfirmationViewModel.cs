// Copyright (c) Microsoft Corporation.
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
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = "Date de naissance")]
        public DateTime? Birthdate { get; set; }

        [Required]
        [Display(Name = "Prénom")]
        public string GivenName { get; set; }

        [Required]
        [Display(Name = "Nom")]
        public string FamilyName { get; set; }

        [Display(Name = "Nom d'usage")]
        public string PreferredName { get; set; }
        
    }
}
