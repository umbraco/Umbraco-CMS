using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

/// <summary>
///     Service implementation for importing member types from XML files.
/// </summary>
public class MemberTypeImportService : IMemberTypeImportService
{
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly IEntityService _entityService;
    private readonly ITemporaryFileToXmlImportService _temporaryFileToXmlImportService;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeImportService"/> class.
    /// </summary>
    /// <param name="packageDataInstallation">The package data installation service for importing member types.</param>
    /// <param name="entityService">The entity service for checking existence of entities.</param>
    /// <param name="temporaryFileToXmlImportService">The service for loading XML from temporary files.</param>
    /// <param name="coreScopeProvider">The core scope provider for creating database scopes.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to user IDs.</param>
    public MemberTypeImportService(
        IPackageDataInstallation packageDataInstallation,
        IEntityService entityService,
        ITemporaryFileToXmlImportService temporaryFileToXmlImportService,
        ICoreScopeProvider coreScopeProvider,
        IUserIdKeyResolver userIdKeyResolver)
    {
        _packageDataInstallation = packageDataInstallation;
        _entityService = entityService;
        _temporaryFileToXmlImportService = temporaryFileToXmlImportService;
        _coreScopeProvider = coreScopeProvider;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <inheritdoc />
    public async Task<Attempt<IMemberType?, MemberTypeImportOperationStatus>> Import(
        Guid temporaryFileId,
        Guid userKey,
        Guid? memberTypeId = null)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        _temporaryFileToXmlImportService.CleanupFileIfScopeCompletes(temporaryFileId);
        Attempt<XElement?, TemporaryFileXmlImportOperationStatus> loadXmlAttempt =
            await _temporaryFileToXmlImportService.LoadXElementFromTemporaryFileAsync(temporaryFileId);
        if (loadXmlAttempt.Success is false)
        {
            return Attempt.FailWithStatus<IMemberType?, MemberTypeImportOperationStatus>(
                loadXmlAttempt.Status is TemporaryFileXmlImportOperationStatus.TemporaryFileNotFound
                    ? MemberTypeImportOperationStatus.TemporaryFileNotFound
                    : MemberTypeImportOperationStatus.TemporaryFileConversionFailure,
                null);
        }

        Attempt<UmbracoEntityTypes> packageEntityTypeAttempt = _temporaryFileToXmlImportService.GetEntityType(loadXmlAttempt.Result!);
        if (packageEntityTypeAttempt.Success is false ||
            packageEntityTypeAttempt.Result is not UmbracoEntityTypes.MemberType)
        {
            return Attempt.FailWithStatus<IMemberType?, MemberTypeImportOperationStatus>(
                MemberTypeImportOperationStatus.TypeMismatch,
                null);
        }

        Guid packageEntityKey = _packageDataInstallation.GetContentTypeKey(loadXmlAttempt.Result!);
        if (memberTypeId is not null && memberTypeId.Equals(packageEntityKey) is false)
        {
            return Attempt.FailWithStatus<IMemberType?, MemberTypeImportOperationStatus>(
                MemberTypeImportOperationStatus.IdMismatch,
                null);
        }

        var entityExits = _entityService.Exists(
            _packageDataInstallation.GetContentTypeKey(loadXmlAttempt.Result!),
            UmbracoObjectTypes.MemberType);
        if (entityExits && memberTypeId is null)
        {
            return Attempt.FailWithStatus<IMemberType?, MemberTypeImportOperationStatus>(
                MemberTypeImportOperationStatus.MemberTypeExists,
                null);
        }

        IReadOnlyList<IMemberType> importResult =
            _packageDataInstallation.ImportMemberTypes(new[] { loadXmlAttempt.Result! }, await _userIdKeyResolver.GetAsync(userKey));

        scope.Complete();

        return Attempt.SucceedWithStatus<IMemberType?, MemberTypeImportOperationStatus>(
            entityExits
                ? MemberTypeImportOperationStatus.SuccessUpdated
                : MemberTypeImportOperationStatus.SuccessCreated,
            importResult[0]);
    }
}
