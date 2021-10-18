// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebApp_Service_Provider_DotNet.ViewModels.Data
{
    public class DgfipResourceViewModel : BaseResourceViewModel
    {
        public string Rfr { get; set; }
        public char SitFam { get; set; }
        public string NbPart { get; set; }
        public Pac Pac { get; set; }
        public string aft { get; set; }
    }

    public class Pac
    {
        public string NbPac { get; set; }
    }
}
