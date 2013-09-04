using System;
using System.Collections.Specialized;
using System.Collections;
using System.Globalization;
using System.Web.Hosting;
using System.Configuration.Provider;

namespace umbraco.providers
{
    /// <summary>
    /// Security Helper Methods
    /// </summary>
    [ObsoleteAttribute("This clas is obsolete and wil be removed along with the legacy Membership Provider.", false)] 
    internal class SecUtility
    {
        /// <summary>
        /// Gets the boolean value.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        internal static bool GetBooleanValue(NameValueCollection config, string valueName, bool defaultValue)
        {
            bool flag;
            string str = config[valueName];
            if (str == null)
                return defaultValue;

            if (!bool.TryParse(str, out flag))
            {
                throw new ProviderException("Value must be boolean.");
            }
            return flag;
        }

        /// <summary>
        /// Checks the array parameter.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <param name="checkForNull">if set to <c>true</c> [check for null].</param>
        /// <param name="checkIfEmpty">if set to <c>true</c> [check if empty].</param>
        /// <param name="checkForCommas">if set to <c>true</c> [check for commas].</param>
        /// <param name="maxSize">Size of the max.</param>
        /// <param name="paramName">Name of the param.</param>
        internal static void CheckArrayParameter(ref string[] param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (param.Length < 1)
            {
                throw new ArgumentException("Parameter array empty.");
            }
            Hashtable hashtable = new Hashtable(param.Length);
            for (int i = param.Length - 1; i >= 0; i--)
            {
                CheckParameter(ref param[i], checkForNull, checkIfEmpty, checkForCommas, maxSize, paramName + "[ " + i.ToString(CultureInfo.InvariantCulture) + " ]");
                if (hashtable.Contains(param[i]))
                {
                    throw new ArgumentException("Parameter duplicate array element");
                }
                hashtable.Add(param[i], param[i]);
            }
        }

        /// <summary>
        /// Checks the parameter.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <param name="checkForNull">if set to <c>true</c> [check for null].</param>
        /// <param name="checkIfEmpty">if set to <c>true</c> [check if empty].</param>
        /// <param name="checkForCommas">if set to <c>true</c> [check for commas].</param>
        /// <param name="maxSize">Size of the max.</param>
        /// <param name="paramName">Name of the param.</param>
        internal static void CheckParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                if (checkForNull)
                {
                    throw new ArgumentNullException(paramName);
                }
            }
            else
            {
                param = param.Trim();
                if (checkIfEmpty && (param.Length < 1))
                {
                    throw new ProviderException("Parameter can not be empty.");
                }
                if ((maxSize > 0) && (param.Length > maxSize))
                {
                    throw new ProviderException("Parameter too long.");
                }
                if (checkForCommas && param.Contains(","))
                {
                    throw new ProviderException("Parameter cannot contain comman.");
                }
            }
        }

        /// <summary>
        /// Checks the password parameter.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <param name="maxSize">Size of the max.</param>
        /// <param name="paramName">Name of the param.</param>
        internal static void CheckPasswordParameter(ref string param, int maxSize, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (param.Length < 1)
            {
                throw new ProviderException("Parameter can not be empty");
            }
            if ((maxSize > 0) && (param.Length > maxSize))
            {
                throw new ProviderException("Parameter too long");
            }
        }

        /// <summary>
        /// Gets the name of the default app.
        /// </summary>
        /// <returns></returns>
        internal static string GetDefaultAppName()
        {
            try
            {
                string applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
                if (string.IsNullOrEmpty(applicationVirtualPath))
                {
                    return "/";
                }
                return applicationVirtualPath;
            }
            catch
            {
                return "/";
            }
        }

        /// <summary>
        /// Gets the int value.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="zeroAllowed">if set to <c>true</c> [zero allowed].</param>
        /// <param name="maxValueAllowed">The max value allowed.</param>
        /// <returns></returns>
        internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
        {
            int num;
            string s = config[valueName];
            if (s == null)
            {
                return defaultValue;
            }
            if (!int.TryParse(s, out num))
            {
                if (zeroAllowed)
                {
                    throw new ProviderException("Value must be non negative integer");
                }
                throw new ProviderException("Value must be positive integer");
            }
            if (zeroAllowed && (num < 0))
            {
                throw new ProviderException("Value must be non negativeinteger");
            }
            if (!zeroAllowed && (num <= 0))
            {
                throw new ProviderException("Value must be positive integer");
            }
            if ((maxValueAllowed > 0) && (num > maxValueAllowed))
            {
                throw new ProviderException("Value too big");
            }
            return num;
        }

        /// <summary>
        /// Validates the parameter.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <param name="checkForNull">if set to <c>true</c> [check for null].</param>
        /// <param name="checkIfEmpty">if set to <c>true</c> [check if empty].</param>
        /// <param name="checkForCommas">if set to <c>true</c> [check for commas].</param>
        /// <param name="maxSize">Size of the max.</param>
        /// <returns></returns>
        internal static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }
            param = param.Trim();
            return (((!checkIfEmpty || (param.Length >= 1)) && ((maxSize <= 0) || (param.Length <= maxSize))) && (!checkForCommas || !param.Contains(",")));
        }


        /// <summary>
        /// Validates the password parameter.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <param name="maxSize">Size of the max.</param>
        /// <returns></returns>
        internal static bool ValidatePasswordParameter(ref string param, int maxSize)
        {
            if (param == null)
            {
                return false;
            }
            if (param.Length < 1)
            {
                return false;
            }
            if ((maxSize > 0) && (param.Length > maxSize))
            {
                return false;
            }
            return true;
        }
    }
}
