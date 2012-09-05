using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.helpers
{
	//TODO: Most of this logic now exists in the Umbraco.Core string extensions, whatever logic exists here that is not there
	// should be moved there and this should be obsoleted.

    public class Casing
    {
        public const string VALID_ALIAS_CHARACTERS = "_-abcdefghijklmnopqrstuvwxyz1234567890";
        public const string INVALID_FIRST_CHARACTERS = "01234567890";

        /// <summary>
        /// A helper method to ensure that an Alias string doesn't contains any illegal characters
        /// which is defined in a private constant 'ValidCharacters' in this class. 
        /// Conventions over configuration, baby. You can't touch this - MC Hammer!
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>An alias guaranteed not to contain illegal characters</returns>
        public static string SafeAlias(string alias)
        {
            StringBuilder safeString = new StringBuilder();
            int aliasLength = alias.Length;
            for (int i = 0; i < aliasLength;i++ )
            {
                string currentChar = alias.Substring(i, 1);
                if (VALID_ALIAS_CHARACTERS.Contains(currentChar.ToLower()))
                {
                    // check for camel (if previous character is a space, we'll upper case the current one
                    if (safeString.Length == 0 && INVALID_FIRST_CHARACTERS.Contains(currentChar.ToLower()))
                    {
                        currentChar = "";
                    }
                    else
                    {
                        // first char should always be lowercase (camel style)
                        // Skipping this check as it can cause incompatibility issues with 3rd party packages
//                        if (safeString.Length == 0)
//                            currentChar = currentChar.ToLower();
                        if (i < aliasLength - 1 && i > 0 && alias.Substring(i - 1, 1) == " ")
                            currentChar = currentChar.ToUpper();

                        safeString.Append(currentChar);
                    }
                }
            }

            return safeString.ToString();
        }

        public static string SafeAliasWithForcingCheck(string alias)
        {
            if (UmbracoSettings.ForceSafeAliases)
            {
                return SafeAlias(alias);
            }
            else
            {
                return alias;
            }
        }

        public static string SpaceCamelCasing(string text)
        {
            string s = text;

            if (2 > s.Length)
            {
                return s;
            }


            var sb = new System.Text.StringBuilder();
            var ca = s.ToCharArray();
            ca[0] = char.ToUpper(ca[0]);

            sb.Append(ca[0]);
            for (int i = 1; i < ca.Length - 1; i++)
            {
                char c = ca[i];
                if (char.IsUpper(c) && (char.IsLower(ca[i + 1]) || char.IsLower(ca[i - 1])))
                {
                    sb.Append(' ');
                }
                sb.Append(c);
            }
            sb.Append(ca[ca.Length - 1]);
            return sb.ToString();

        }
    }
}
