using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IStylesheetRepository : IRepository<string, Stylesheet>
    {
        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> that exist at the relative path specified. 
        /// </summary>
        /// <param name="rootPath">
        /// If null or not specified, will return the stylesheets at the root path relative to the IFileSystem
        /// </param>
        /// <returns></returns>
        IEnumerable<Stylesheet> GetStylesheetsAtPath(string rootPath = null);


        bool ValidateStylesheet(Stylesheet stylesheet);
    }
}