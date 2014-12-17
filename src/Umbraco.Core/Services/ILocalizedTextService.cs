using System.Globalization;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// The entry point to localize any key in the text storage source for a given culture 
    /// </summary>
    /// <remarks>
    /// This class is created to be as simple as possible so that it can be replaced very easily, 
    /// all other methods are extension methods that simply call the one underlying method in this class
    /// </remarks>
    public interface ILocalizedTextService
    {
        /// <summary>
        /// Localize a key with variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="culture"></param>
        /// <param name="variables">This can be null</param>
        /// <returns></returns>
        string Localize(string key, CultureInfo culture, 
            
            //TODO: Potentially this should be a dictionary to simplify things a little?
            object variables);
    }
}
