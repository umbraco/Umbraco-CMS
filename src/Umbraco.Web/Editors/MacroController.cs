using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// API controller to deal with Macro data
    /// </summary>
    [PluginController("UmbracoApi")]
    public class MacroController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Gets the macro parameters to be filled in for a particular macro
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Note that ALL logged in users have access to this method because editors will need to isnert macros into rte (content/media/members) and it's used for 
        /// inserting into templates/views/etc... it doesn't expose any sensitive data.
        /// </remarks>
        public IEnumerable<MacroParameter> GetMacroParameters(int macroId)
        {
            var macro = Services.MacroService.GetById(macroId);
            if (macro == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<IEnumerable<MacroParameter>>(macro);
        }
    }
}