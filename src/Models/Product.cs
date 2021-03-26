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

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace ProductStore.Models
{
    [Serializable]
    [DataContract(Name = "product", Namespace = "http://windowsazure.cat.microsoft.com/samples/servicebus")]
    public class Product
    {
        #region Public Properties
        [JsonProperty(PropertyName = "productId", Order = 1)]
        [DataMember(Name = "productId", Order = 1)]
        public int ProductId { get; set; }
        
        [JsonProperty(PropertyName = "name", Order = 2)]
        [DataMember(Name = "name", Order = 2)]
        public string Name { get; set; }
        
        [JsonProperty(PropertyName = "category", Order = 3)]
        [DataMember(Name = "category", Order = 3)]
        public string Category { get; set; }
        
        [JsonProperty(PropertyName = "price", Order = 4)]
        [DataMember(Name = "price", Order = 4)]
        public decimal Price { get; set; }
        #endregion
    }
}
