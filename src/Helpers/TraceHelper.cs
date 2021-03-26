#region Copyright
//=======================================================================================
// Windows Azure Customer Advisory Team  
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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using ProductStore.Properties;
#endregion

namespace ProductStore.Helpers
{
    /// <summary>
    /// This class is used to trace messages
    /// </summary>
    public static class TraceHelper
    {
        #region Static Constructor
        /// <summary>
        /// The static constructor initializes the TraceSwitch used to determine which messages to trace at runtime.
        /// </summary>
        static TraceHelper()
        {
            TraceSwitch = new TraceSwitch(Resources.TraceSwitchName, Resources.TraceSwitchDescription);
        }
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Gets and sets the TraceSwitch
        /// </summary>
        public static TraceSwitch TraceSwitch { get; }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Invoked at the entrance of a method.
        /// </summary>
        public static void TraceIn()
        {
            if (TraceSwitch.TraceVerbose)
            {
                Trace.TraceInformation(Resources.TraceInFormat, GetFullMethodName(GetCallingMethod()), Resources.In);
            }
        }

        /// <summary>
        /// Invoked at the exit of a method.
        /// </summary>
        public static void TraceOut()
        {
            if (TraceSwitch.TraceVerbose)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                                              Resources.TraceOutFormat,
                                              GetFullMethodName(GetCallingMethod()),
                                              Resources.Out));
            }
        }

        /// <summary>
        /// Writes a formatted information message to the trace. 
        /// </summary>
        /// <param name="format">A string containing zero or more format items.</param>
        /// <param name="parameters">A list containing zero or more data items to format.</param>
        public static void TraceInfo(string format, params object[] parameters)
        {
            //if (!TraceSwitch.TraceInfo)
            //{
            //    return;
            //}
            if (((parameters != null) && (parameters.Length > 0)))
            {
                Trace.TraceInformation(format, parameters);
            }
            else
            {
                Trace.TraceInformation(format);
            }
        }

        /// <summary>
        /// Writes a formatted warning message to the trace. 
        /// </summary>
        /// <param name="format">A string containing zero or more format items.</param>
        /// <param name="parameters">A list containing zero or more data items to format.</param>
        public static void TraceWarning(string format, params object[] parameters)
        {
            //if (!TraceSwitch.TraceWarning)
            //{
            //    return;
            //}
            if (((parameters != null) && (parameters.Length > 0)))
            {
                Trace.TraceWarning(format, parameters);
            }
            else
            {
                Trace.TraceWarning(format);
            }
        }

        /// <summary>
        /// Writes a formatted error message to the trace. 
        /// </summary>
        /// <param name="format">A string containing zero or more format items.</param>
        /// <param name="parameters">A list containing zero or more data items to format.</param>
        public static void TraceError(string format, params object[] parameters)
        {
            //if (!TraceSwitch.TraceError)
            //{
            //    return;
            //}
            if (((parameters != null) && (parameters.Length > 0)))
            {
                Trace.TraceError(format, parameters);
            }
            else
            {
                Trace.TraceError(format);
            }
        }

        public static void TraceError(Exception ex)
        {
            //if (!TraceSwitch.TraceError)
            //{
            //    return;
            //}
            if (string.IsNullOrWhiteSpace(ex?.Message))
            {
                return;
            }
            Trace.TraceError(Resources.ExceptionFormat, 
                             GetFullMethodName(GetCallingMethod()),
                             ex.Message,
                             !string.IsNullOrWhiteSpace(ex.InnerException?.Message) ?  
                             ex.InnerException.Message :
                             Resources.Null);
            
        }

        /// <summary>
        /// Writes a formatted information message to the trace. 
        /// </summary>
        /// <param name="format">A string containing zero or more format items.</param>
        /// <param name="parameters">A list containing zero or more data items to format.</param>
        public static void TraceVerbose(string format, params object[] parameters)
        {
            //if (!TraceSwitch.TraceVerbose)
            //{
            //    return;
            //}
            if (((parameters != null) && (parameters.Length > 0)))
            {
                Trace.TraceInformation(format, parameters);
            }
            else
            {
                Trace.TraceInformation(format);
            }
        }
        #endregion

        #region Private Static Methods
        public static string GetFullMethodName(MethodBase callingMethod)
        {
            if ((callingMethod == null) || (callingMethod.DeclaringType == null))
            {
                return Resources.Unknown;
            }
            return string.Format(CultureInfo.InvariantCulture, Resources.MethodNameFormat, callingMethod.DeclaringType.Name, callingMethod.Name);
        }

        public static MethodBase GetCallingMethod()
        {
            var frames = new StackTrace().GetFrames();
            return frames == null || frames.Length < 3 ? null : frames[2].GetMethod();
        }
        #endregion
    }
}
