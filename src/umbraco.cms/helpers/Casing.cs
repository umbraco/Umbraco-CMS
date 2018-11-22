using System;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace umbraco.cms.helpers
{
	//TODO: Most of this logic now exists in the Umbraco.Core string extensions, whatever logic exists here that is not there
	// should be moved there and this should be obsoleted.

    public class Casing
    {
        [Obsolete("Use Umbraco.Core.StringExtensions.UmbracoValidAliasCharacters instead")]
        public const string VALID_ALIAS_CHARACTERS = "_-abcdefghijklmnopqrstuvwxyz1234567890";

        [Obsolete("Use Umbraco.Core.StringExtensions.UmbracoInvalidFirstCharacters instead")]
        public const string INVALID_FIRST_CHARACTERS = "0123456789";

        /// <summary>
        /// A helper method to ensure that an Alias string doesn't contains any illegal characters
        /// which is defined in a private constant 'ValidCharacters' in this class. 
        /// Conventions over configuration, baby. You can't touch this - MC Hammer!
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>An alias guaranteed not to contain illegal characters</returns>
        [Obsolete("Use Umbraco.Core.StringExtensions.ToSafeAlias instead")]
        public static string SafeAlias(string alias)
        {
            return alias.ToSafeAlias(); // and anyway the code is the same
        }

        [Obsolete("Use Umbraco.Core.StringExtensions.ToSafeAliasWithForcingCheck instead")]
        public static string SafeAliasWithForcingCheck(string alias)
        {
            return alias.ToSafeAliasWithForcingCheck(); // and anyway the code is the same
        }

        //NOTE: Not sure what this actually does but is used a few places, need to figure it out and then move to StringExtensions and obsolete.
        // it basically is yet another version of SplitPascalCasing
        // plugging string extensions here to be 99% compatible
        // the only diff. is with numbers, Number6Is was "Number6 Is", and the new string helper does it too,
        // but the legacy one does "Number6Is"... assuming it is not a big deal.

        [UmbracoWillObsolete("We should really obsolete that one too.")]
        public static string SpaceCamelCasing(string text)
        {
            return text.Length < 2 ? text : text.SplitPascalCasing().ToFirstUpperInvariant();
        }
    }
}
