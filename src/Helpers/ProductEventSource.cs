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
using System.Diagnostics.Tracing;
#endregion

namespace ProductStore.Helpers
{
    [EventSource(Name = "ProductEventSource")]
    public sealed class ProductEventSource: EventSource
    {
        public class Keywords
        {
            public const EventKeywords Page = (EventKeywords)1;
            public const EventKeywords DataBase = (EventKeywords)2;
            public const EventKeywords Diagnostic = (EventKeywords)4;
            public const EventKeywords Performance = (EventKeywords)8;
        }

        public class Tasks
        {
            public const EventTask Page = (EventTask)1;
            public const EventTask DbOperation = (EventTask)2;
            public const EventTask Exception = (EventTask)3;
        }

        #region Public Static Fields
        public static ProductEventSource Log = new ProductEventSource(); 
        #endregion

        #region Public Methods
        [Event(1, 
               Message = "Product Added", 
               Keywords = Keywords.DataBase,
               Task = Tasks.DbOperation,
               Opcode = EventOpcode.Receive,
               Level = EventLevel.Informational)]
        public void ProductAdded(int productId, string name, string category, double price)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category))
            {
                return;
            }
            if (IsEnabled())
            {
                WriteEvent(1, productId, name, category, price);
            }
        }

        [Event(2, 
               Message = "Product Updated", 
               Keywords = Keywords.DataBase,
               Task = Tasks.DbOperation,
               Opcode = EventOpcode.Send,
               Level = EventLevel.Informational)]
        public void ProductUpdated(int productId, string name, string category, double price)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category))
            {
                return;
            }
            if (IsEnabled())
            {
                WriteEvent(2, productId, name, category, price);
            }
        }

        [Event(3, 
               Message = "Product Deleted",
               Keywords = Keywords.DataBase,
               Task = Tasks.DbOperation,
               Opcode = EventOpcode.Extension,
               Level = EventLevel.Informational)]
        public void ProductDeleted(int productId)
        {
            if (IsEnabled())
            {
                WriteEvent(3, productId);
            }
        }

        [Event(4, 
               Message = "Exception Occurred",
               Keywords = Keywords.Diagnostic,
               Task = Tasks.Exception,
               Opcode = EventOpcode.Info,
               Level = EventLevel.Error)]
        public void ExceptionOccurred(string exception, string innerException)
        {
            if (string.IsNullOrWhiteSpace(exception))
            {
                return;
            }
            if (IsEnabled())
            {
                WriteEvent(4, exception, string.IsNullOrWhiteSpace(innerException) ? string.Empty : innerException);
            }
        } 
        #endregion
    }
}