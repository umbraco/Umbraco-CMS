namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a Template File (Masterpage or Mvc View)
    /// </summary>
    public interface ITemplate : IFile
    {
        /// <summary>
        /// Returns the <see cref="RenderingEngine"/> that corresponds to the template file
        /// </summary>
        /// <returns><see cref="RenderingEngine"/></returns>
        RenderingEngine GetTypeOfRenderingEngine();

        /// <summary>
        /// Set the mastertemplate
        /// </summary>
        /// <param name="masterTemplate"></param>
        void SetMasterTemplate(ITemplate masterTemplate);
    }
}