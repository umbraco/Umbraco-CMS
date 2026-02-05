using System.Xml.Linq;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

/// <summary>
///     Service for loading and analyzing XML import data from temporary files.
/// </summary>
/// <remarks>
///     This service provides functionality to convert temporary files containing XML definitions
///     into <see cref="XElement"/> objects for further processing during content type imports.
/// </remarks>
public interface ITemporaryFileToXmlImportService
{
    /// <summary>
    ///     Loads the XML content from a temporary file and returns it as an <see cref="XElement"/>.
    /// </summary>
    /// <param name="temporaryFileId">The unique identifier of the temporary file to load.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult, TStatus}"/>
    ///     with the loaded <see cref="XElement"/> on success, or <c>null</c> with an appropriate
    ///     <see cref="TemporaryFileXmlImportOperationStatus"/> on failure.
    /// </returns>
    /// <remarks>
    ///     Only if this method is called within a scope, the temporary file will be cleaned up if that scope completes.
    /// </remarks>
    Task<Attempt<XElement?, TemporaryFileXmlImportOperationStatus>> LoadXElementFromTemporaryFileAsync(
        Guid temporaryFileId);

    /// <summary>
    ///     Determines the Umbraco entity type from an XML element.
    /// </summary>
    /// <param name="entityElement">The XML element to analyze.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult}"/> containing the <see cref="UmbracoEntityTypes"/> value
    ///     on success, or a failed attempt if the entity type could not be determined.
    /// </returns>
    Attempt<UmbracoEntityTypes> GetEntityType(XElement entityElement);

    /// <summary>
    ///     Analyzes a temporary file containing XML import data and returns information about the entity that would be imported.
    /// </summary>
    /// <param name="temporaryFileId">The unique identifier of the temporary file to analyze.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult, TStatus}"/>
    ///     with an <see cref="EntityXmlAnalysis"/> on success containing the entity type, alias, and key;
    ///     or <c>null</c> with an appropriate <see cref="TemporaryFileXmlImportOperationStatus"/> on failure.
    /// </returns>
    /// <remarks>
    ///     As this method does not persist anything, no scope is created and the temporary file is not cleaned up.
    /// </remarks>
    Task<Attempt<EntityXmlAnalysis?, TemporaryFileXmlImportOperationStatus>> AnalyzeAsync(
        Guid temporaryFileId);

    /// <summary>
    ///     Registers the temporary file for cleanup when the current scope completes successfully.
    /// </summary>
    /// <param name="temporaryFileId">The unique identifier of the temporary file to clean up.</param>
    void CleanupFileIfScopeCompletes(Guid temporaryFileId);
}
