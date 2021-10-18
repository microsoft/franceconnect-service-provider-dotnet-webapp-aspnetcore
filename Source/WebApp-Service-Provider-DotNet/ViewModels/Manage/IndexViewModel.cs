// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace WebApp_Service_Provider_DotNet.ViewModels.Manage
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }
        
        public bool BrowserRemembered { get; set; }
    }
}
