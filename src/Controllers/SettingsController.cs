#region Copyright
//=======================================================================================
//Microsoft Windows Server AppFabric Product Advisory Team (CAT)  
//
// This sample is supplemental to the technical guidance published on the community
// blog at http://blogs.msdn.com/b/paolos/. 
// 
// Author: Paolo Salvatori
//=======================================================================================
// Copyright © 2011 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
#endregion

#region Using Directives

using System.Collections.Generic;
using System.Web.Http;

#endregion


namespace ProductStore.Controllers
{
    public class SettingsController : ApiController
    {
        #region Public Fields
        public Dictionary<string, string> SettingDictionary = new Dictionary<string, string>();
        #endregion

        #region Public Methods
        // GET: api/settings
        public IDictionary<string, string> Get()
        {
            return SettingDictionary;
        } 
        #endregion
    }
}
