using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.helpers
{
    public class Casing
    {
        private const string ValidCharacters = "_-abcdefghijklmnopqrstuvwxyz";

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
                if (ValidCharacters.Contains(currentChar.ToLower()))
                {
                    // check for camel (if previous character is a space, we'll upper case the current one
                    if (i < aliasLength - 1 && i > 0 && alias.Substring(i - 1, 1) == " ")
                        currentChar = currentChar.ToUpper();

                    safeString.Append(currentChar);
                }
            }

            return safeString.ToString();
        }
    }
}
