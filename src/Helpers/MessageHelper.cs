#region Copyright
//=======================================================================================
// Author: Paolo Salvatori
// GitHub: https://github.com/paolosalvatori
//=======================================================================================
// Copyright © 2021 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
#endregion

#region Using Directives
using System; 
#endregion

namespace Products.Helpers
{
    /// <summary>
    /// This static class contains helper methods for exception messages
    /// </summary>
    public static class MessageHelper
    {
        public static string FormatException(Exception ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }
            var baseException = ex.GetBaseException();
            if (string.Compare(ex.Message, baseException?.Message, true) == 0)
            {
                return $"An error occurred: Exception=[{ex.Message}]";
            }
            return $"An error occurred: Exception=[{ex.Message}] BaseException=[{baseException?.Message ?? "NULL"}]";
        }
    }
}
