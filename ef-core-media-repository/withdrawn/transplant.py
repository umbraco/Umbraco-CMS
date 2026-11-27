"""PR 5 transplant: hoist the generic service engine into ContentServiceBase<TContent, TContentType>.

Slices member blocks by line range out of PublishableContentServiceBase.cs and ContentService.cs (as they
stand at the PR 4 head), applies the listed substitutions, and writes the three files. Every substitution
asserts its expected occurrence count so a drifted source line fails loudly instead of silently.
"""
import re
import sys
from pathlib import Path

ROOT = Path(sys.argv[1])
SERVICES = ROOT / "src" / "Umbraco.Core" / "Services"
PCSB = SERVICES / "PublishableContentServiceBase.cs"
CS = SERVICES / "ContentService.cs"
WIDE = SERVICES / "ContentServiceBase.cs"
NL = "\r\n"


def read_lines(path: Path) -> list[str]:
    text = path.read_text(encoding="utf-8", newline="")
    assert "\r\n" in text and "\n" not in text.replace("\r\n", ""), f"{path} is not CRLF"
    lines = text.split("\r\n")
    assert lines[-1] == "", f"{path} has no trailing newline"
    return lines[:-1]


def block(lines: list[str], start: int, end: int) -> str:
    """1-based inclusive line range, as a CRLF-joined string without a trailing newline."""
    return NL.join(lines[start - 1:end])


def sub(text: str, old: str, new: str, count: int = 1) -> str:
    found = text.count(old)
    assert found == count, f"expected {count} of {old!r}, found {found}"
    return text.replace(old, new)


def resub(text: str, pattern: str, new: str, count: int) -> str:
    found = len(re.findall(pattern, text))
    assert found == count, f"expected {count} of /{pattern}/, found {found}"
    return re.sub(pattern, new, text)


def generic(text: str) -> str:
    """Retype a ContentService block for the wide base."""
    text = re.sub(r"\bIContentType\b", "TContentType", text)
    text = re.sub(r"\bIContent\b", "TContent", text)
    text = text.replace("scope.WriteLock(Constants.Locks.ContentTree);", "scope.WriteLock(WriteLockIds);")
    text = text.replace("scope.ReadLock(Constants.Locks.ContentTree);", "scope.ReadLock(ReadLockIds);")
    text = text.replace("_documentRepository.", "_contentRepository.")
    text = text.replace("new ContentTreeChangeNotification(", "CreateTreeChangeNotification(")
    return text


def doc(*summary_lines: str) -> str:
    return NL.join("    /// " + line if line else "    ///" for line in summary_lines)


def with_doc(text: str, signature: str, docblock: str) -> str:
    """Replace the `/// <inheritdoc />` line directly above the member whose declaration contains `signature`."""
    lines = text.split(NL)
    hits = [i for i, line in enumerate(lines) if signature in line]
    assert len(hits) == 1, f"expected one line containing {signature!r}, found {len(hits)}"
    index = hits[0]
    assert lines[index - 1].strip() in ("/// <inheritdoc />", "/// <inheritdoc/>"), f"no inheritdoc above {signature!r}: {lines[index - 1]!r}"
    lines[index - 1] = docblock
    return NL.join(lines)


pcsb = read_lines(PCSB)
cs = read_lines(CS)

# ---------------------------------------------------------------------------------------------------------
# Blocks hoisted from PublishableContentServiceBase (line numbers as of 83630724b17).
# ---------------------------------------------------------------------------------------------------------
P = {
    "hooks_object": block(pcsb, 86, 92),
    "hook_logger": block(pcsb, 96, 96),
    "hook_delete_locked": block(pcsb, 132, 132),
    "hooks_saving_saved": block(pcsb, 134, 140),
    "hook_tree_change_single": block(pcsb, 142, 142),
    "hook_tree_change_many": block(pcsb, 146, 146),
    "hook_deleting": block(pcsb, 148, 148),
    "hooks_versions": block(pcsb, 164, 182),
    "hooks_saved_cultures": block(pcsb, 184, 212),
    "build_culture_map": block(pcsb, 247, 274),
    "count": block(pcsb, 339, 390),
    "get_by_id": block(pcsb, 396, 406),
    "get_by_ids": block(pcsb, 408, 426),
    "get_by_ids_aliases_templates": block(pcsb, 428, 446),
    "paged_of_type": block(pcsb, 473, 483),
    "paged_of_types": block(pcsb, 495, 505),
    "versions": block(pcsb, 507, 545),
    "has_children": block(pcsb, 585, 592),
    "get_parent": block(pcsb, 604, 630),
    "save_single": block(pcsb, 636, 705),
    "save_many": block(pcsb, 707, 759),
    "delete": block(pcsb, 1747, 1781),
    "delete_versions": block(pcsb, 1783, 1821),
    "delete_version": block(pcsb, 1823, 1870),
    "has_unsaved_changes": block(pcsb, 1876, 1876),
    "check_data_integrity": block(pcsb, 1888, 1907),
    "audit": block(pcsb, 1962, 1989),
    "delete_of_types": block(pcsb, 1995, 1996),
    "get_content_type": block(pcsb, 1998, 2019),
}
P_REMOVED = [
    (86, 92), (96, 96), (132, 132), (134, 140), (142, 142), (146, 146), (148, 148), (164, 182), (184, 212),
    (247, 274), (325, 326), (339, 392), (396, 426), (473, 483), (495, 545), (585, 592), (604, 630),
    (636, 759), (1745, 1872), (1876, 1876), (1888, 1907), (1960, 2021),
]

# ---------------------------------------------------------------------------------------------------------
# Blocks hoisted from ContentService.
# ---------------------------------------------------------------------------------------------------------
C = {
    "create": block(cs, 144, 256),
    "throw_if_save_rejected": block(cs, 258, 271),
    "get_children": block(cs, 301, 311),
    "get_descendants": block(cs, 325, 335),
    "get_parent_by_key": block(cs, 349, 362),
    "get_root": block(cs, 364, 372),
    "paged_recycle_bin": block(cs, 374, 384),
    "move_to_recycle_bin": block(cs, 803, 844),
    "move": block(cs, 846, 953),
    "perform_move_locked": block(cs, 955, 1016),
    "leave_descendant": block(cs, 1018, 1035),
    "perform_move_descendant": block(cs, 1037, 1043),
    "perform_move_content": block(cs, 1045, 1054),
    "empty_recycle_bin": block(cs, 1056, 1120),
    "sort": block(cs, 1349, 1431),
    "sort_children": block(cs, 1433, 1472),
    "delete_of_type": block(cs, 1586, 1588),
}
C_REMOVED = [
    (142, 273), (301, 312), (325, 336), (349, 384), (801, 1122), (1349, 1473), (1586, 1589),
]

# ---------------------------------------------------------------------------------------------------------
# Substitutions on the publishable-origin blocks.
# ---------------------------------------------------------------------------------------------------------
hook_logger = sub(P["hook_logger"], "ILogger<PublishableContentServiceBase<TContent>>", "ILogger<ContentServiceBase<TContent, TContentType>>")

get_by_ids_aliases = P["get_by_ids_aliases_templates"]
get_by_ids_aliases = sub(get_by_ids_aliases, "string[]? propertyAliases, bool loadTemplates, CancellationToken", "string[]? propertyAliases, CancellationToken")
get_by_ids_aliases = sub(get_by_ids_aliases, "GetManyAsync(idsA, propertyAliases, loadTemplates, cancellationToken)", "GetManyAsync(idsA, propertyAliases, cancellationToken)")
get_by_ids_aliases = with_doc(get_by_ids_aliases, "GetByIdsAsync(IEnumerable<Guid> ids, string[]? propertyAliases, CancellationToken", doc(
    "<summary>",
    "    Gets content items by their unique identifiers, loading only the requested properties.",
    "</summary>",
    '<param name="ids">The unique identifiers of the content items.</param>',
    '<param name="propertyAliases">',
    "    The property aliases to load. If <c>null</c>, all properties are loaded.",
    "    If empty, no custom properties are loaded (only system properties).",
    "</param>",
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The content items, in the order requested.</returns>",
))

get_by_ids = with_doc(P["get_by_ids"], "GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken", doc(
    "<summary>",
    "    Gets content items by their unique identifiers.",
    "</summary>",
    '<param name="ids">The unique identifiers of the content items.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The content items, in the order requested.</returns>",
))

paged_of_type = with_doc(P["paged_of_type"], "GetPagedOfTypeAsync(Guid contentTypeKey,", doc(
    "<summary>",
    "    Gets a paged list of content items of a given content type.",
    "</summary>",
    '<param name="contentTypeKey">The Guid key of the content type.</param>',
    '<param name="skip">The number of items to skip.</param>',
    '<param name="take">The maximum number of items to return.</param>',
    '<param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>A paged result containing the matching content items.</returns>",
))

paged_of_types = with_doc(P["paged_of_types"], "GetPagedOfTypesAsync(Guid[] contentTypeKeys,", doc(
    "<summary>",
    "    Gets a paged list of content items whose content type is one of the specified content types.",
    "</summary>",
    '<param name="contentTypeKeys">The keys of the content types.</param>',
    '<param name="skip">The number of items to skip.</param>',
    '<param name="take">The maximum number of items to return.</param>',
    '<param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>A paged result containing the matching content items.</returns>",
))

versions = P["versions"]
versions = with_doc(versions, "GetVersionAsync(int versionId,", doc(
    "<summary>",
    "    Gets a version of a content item.",
    "</summary>",
    "<remarks>",
    "    Int-keyed rather than Guid-keyed: unlike content items, versions have no Guid key resolvable",
    "    from a caller-facing identifier today, so this takes the version's raw database id directly.",
    "</remarks>",
    '<param name="versionId">The version identifier.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The content version, or <c>null</c> if not found.</returns>",
))
versions = with_doc(versions, "GetVersionsAsync(Guid contentKey,", doc(
    "<summary>",
    "    Gets all versions of a content item.",
    "</summary>",
    '<param name="contentKey">The Guid key of the content item.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The content versions.</returns>",
    "<remarks>Versions are ordered with current first, then most recent first.</remarks>",
))
versions = with_doc(versions, "GetVersionsSlimAsync(Guid contentKey,", doc(
    "<summary>",
    "    Gets a page of versions of a content item.",
    "</summary>",
    '<param name="contentKey">The Guid key of the content item.</param>',
    '<param name="skip">The number of versions to skip.</param>',
    '<param name="take">The number of versions to take.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The content versions.</returns>",
    "<remarks>Versions are ordered with current first, then most recent first.</remarks>",
))
versions = with_doc(versions, "GetVersionIdsAsync(Guid contentKey,", doc(
    "<summary>",
    "    Gets a page of version ids of a content item.",
    "</summary>",
    '<param name="contentKey">The Guid key of the content item.</param>',
    '<param name="skip">The number of versions to skip.</param>',
    '<param name="take">The maximum number of version ids to return.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The version ids.</returns>",
    "<remarks>Versions are ordered with current first, then most recent first.</remarks>",
))

save_single = P["save_single"]
save_single = sub(
    save_single,
    "    /// <inheritdoc />" + NL
    + "    public async Task<Attempt<ContentSaveOperationStatus>> SaveAsync(TContent content, Guid userKey, ContentScheduleCollection? contentSchedule, CancellationToken cancellationToken)" + NL
    + "    {" + NL
    + "        PublishedState publishedState = content.PublishedState;" + NL
    + "        if (publishedState is not (PublishedState.Published or PublishedState.Unpublished))" + NL
    + "        {" + NL
    + "            return Attempt.Fail(ContentSaveOperationStatus.InvalidPublishedState);" + NL
    + "        }" + NL,
    doc(
        "<summary>",
        "    Saves a content item.",
        "</summary>",
        '<param name="content">The content item to save.</param>',
        '<param name="userKey">The key of the user performing the action.</param>',
        '<param name="cancellationToken">The cancellation token.</param>',
        "<returns>An attempt carrying the operation status.</returns>",
    ) + NL
    + "    public Task<Attempt<ContentSaveOperationStatus>> SaveAsync(TContent content, Guid userKey, CancellationToken cancellationToken)" + NL
    + "        => PerformSaveAsync(content, userKey, persistAlongside: null, cancellationToken);" + NL
    + NL
    + doc(
        "<summary>",
        "    Saves a content item, running the kind's save validation first and persisting whatever accompanies the",
        "    item in the same scope, before the saved notification is published.",
        "</summary>",
        '<param name="content">The content item to save.</param>',
        '<param name="userKey">The key of the user performing the action.</param>',
        '<param name="persistAlongside">',
        "    What to persist right after the item itself, or <c>null</c> when nothing accompanies it; a publishable kind",
        "    persists the item's schedule here.",
        "</param>",
        '<param name="cancellationToken">The cancellation token.</param>',
        "<returns>An attempt carrying the operation status.</returns>",
    ) + NL
    + "    protected async Task<Attempt<ContentSaveOperationStatus>> PerformSaveAsync(TContent content, Guid userKey, Func<Task>? persistAlongside, CancellationToken cancellationToken)" + NL
    + "    {" + NL
    + "        ContentSaveOperationStatus validationStatus = ValidateForSave(content);" + NL
    + "        if (validationStatus is not ContentSaveOperationStatus.Success)" + NL
    + "        {" + NL
    + "            return Attempt.Fail(validationStatus);" + NL
    + "        }" + NL,
)
save_single = sub(
    save_single,
    "        if (contentSchedule is not null)" + NL
    + "        {" + NL
    + "            await _contentRepository.PersistContentScheduleAsync(content, contentSchedule, cancellationToken);" + NL
    + "        }" + NL,
    "        if (persistAlongside is not null)" + NL
    + "        {" + NL
    + "            await persistAlongside();" + NL
    + "        }" + NL,
)

delete = P["delete"]
delete = with_doc(delete, "DeleteAsync(TContent content, Guid userKey,", doc(
    "<summary>",
    "    Deletes a content item.",
    "</summary>",
    '<param name="content">The content item to delete.</param>',
    '<param name="userKey">The key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>An attempt carrying the operation status.</returns>",
    "<remarks>",
    "    <para>This method will also delete associated media files, child content and possibly associated domains.</para>",
    "    <para>This method entirely clears the content from the database.</para>",
    "</remarks>",
))
delete = sub(
    delete,
    "        // if it's not trashed yet, and published, we should unpublish" + NL
    + "        // but... Unpublishing event makes no sense (not going to cancel?) and no need to save" + NL
    + "        // just raise the event" + NL
    + "        if (content is { Trashed: false, Published: true })" + NL
    + "        {" + NL
    + "            scope.Notifications.Publish(CreateUnpublishedNotification(" + NL
    + "                content," + NL
    + "                eventMessages," + NL
    + '                BuildCultureMap(content, content.ContentType.VariesByCulture() ? content.PublishedCultures : ["*"])));' + NL
    + "        }" + NL,
    "        BeforeDeleteLocked(scope, content, eventMessages);" + NL,
)

delete_version = sub(
    P["delete_version"],
    "        // don't delete the current or published version" + NL
    + "        if (c?.VersionId != versionId &&" + NL
    + "            c?.PublishedVersionId != versionId)" + NL,
    "        // don't delete the current version, nor any other version the kind keeps" + NL
    + "        if (c is null || IsVersionDeletable(c, versionId))" + NL,
)

delete_of_types = with_doc(P["delete_of_types"], "DeleteOfTypesAsync(IEnumerable<Guid> contentTypeKeys,", doc(
    "<summary>",
    "    Deletes all content of given types.",
    "</summary>",
    '<param name="contentTypeKeys">The Guid keys of the content types.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<remarks>",
    "    <para>All non-deleted descendants of the deleted content is moved to the recycle bin.</para>",
    "    <para>This operation is potentially dangerous and expensive.</para>",
    "</remarks>",
    "<returns>An attempt carrying the operation status.</returns>",
))

get_content_type = P["get_content_type"]
get_content_type = sub(get_content_type, "protected async Task<IContentType> GetContentTypeAsync(", "protected async Task<TContentType> GetContentTypeAsync(")
get_content_type = sub(get_content_type, "IContentType? contentType = await _contentTypeRepository.GetAsync(", "TContentType? contentType = await _contentTypeRepository.GetAsync(")

# ---------------------------------------------------------------------------------------------------------
# Substitutions on the ContentService-origin blocks.
# ---------------------------------------------------------------------------------------------------------
create = C["create"]
create = sub(
    create,
    "        Content content = parent is not null" + NL
    + "            ? new Content(name, parent, contentType, userId)" + NL
    + "            : new Content(name, Constants.System.Root, contentType, userId);" + NL,
    "        TContent content = CreateEntity(name, parent, contentType, userId);" + NL,
    count=2,
)
create = sub(create, "        var content = new Content(name, parent, contentType, userId);" + NL, "        TContent content = CreateEntity(name, parent, contentType, userId);" + NL, count=2)
create = sub(create, "await SaveAsync(content, userKey, null, cancellationToken);", "await SaveAsync(content, userKey, cancellationToken);", count=2)
create = generic(create)
create = with_doc(create, "CreateAsync(string name, Guid? parentKey, string contentTypeAlias,", doc(
    "<summary>",
    "    Creates a content item.",
    "</summary>",
    '<param name="name">The name of the content item.</param>',
    '<param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the tree.</param>',
    '<param name="contentTypeAlias">The content type alias.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The created content item.</returns>",
))
create = with_doc(create, "CreateAsync(string name, Guid? parentKey, TContentType contentType,", doc(
    "<summary>",
    "    Creates a content item.",
    "</summary>",
    '<param name="name">The name of the content item.</param>',
    '<param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the tree.</param>',
    '<param name="contentType">The content type.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The created content item.</returns>",
))
create = with_doc(create, "CreateAsync(string name, TContent parent, string contentTypeAlias,", doc(
    "<summary>",
    "    Creates a content item.",
    "</summary>",
    '<param name="name">The name of the content item.</param>',
    '<param name="parent">The parent content item.</param>',
    '<param name="contentTypeAlias">The content type alias.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The created content item.</returns>",
))
create = with_doc(create, "CreateAndSaveAsync(string name, Guid? parentKey, string contentTypeAlias,", doc(
    "<summary>",
    "    Creates and saves a content item.",
    "</summary>",
    '<param name="name">The name of the content item.</param>',
    '<param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the tree.</param>',
    '<param name="contentTypeAlias">The content type alias.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The created and saved content item.</returns>",
    '<exception cref="InvalidOperationException">The content item cannot be saved because it fails the kind\'s save validation.</exception>',
    "<remarks>",
    "    When a saving notification cancels the save, the content item is returned unsaved (without an identity).",
    "</remarks>",
))
create = with_doc(create, "CreateAndSaveAsync(string name, TContent parent, string contentTypeAlias,", doc(
    "<summary>",
    "    Creates and saves a content item.",
    "</summary>",
    '<param name="name">The name of the content item.</param>',
    '<param name="parent">The parent content item.</param>',
    '<param name="contentTypeAlias">The content type alias.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The created and saved content item.</returns>",
    '<exception cref="InvalidOperationException">The content item cannot be saved because it fails the kind\'s save validation.</exception>',
    "<remarks>",
    "    When a saving notification cancels the save, the content item is returned unsaved (without an identity).",
    "</remarks>",
))

throw_if_save_rejected = (
    "    // Saving rejects an invalid name with a status; creating-and-saving surfaces it as an exception so the caller" + NL
    + "    // is never handed back an item that was silently left unsaved. A kind with further save validation extends it." + NL
    + "    protected virtual void ThrowIfSaveRejected(TContent content, Attempt<ContentSaveOperationStatus> saveResult)" + NL
    + "    {" + NL
    + "        if (saveResult.Result is ContentSaveOperationStatus.InvalidName)" + NL
    + "        {" + NL
    + "            throw new InvalidOperationException(" + NL
    + '                $"Content with the name {content.Name} cannot be more than 255 characters in length.");' + NL
    + "        }" + NL
    + "    }"
)

get_children = with_doc(generic(C["get_children"]), "GetChildrenAsync(Guid? parentKey,", doc(
    "<summary>",
    "    Gets a paged list of direct children of a content node.",
    "</summary>",
    '<param name="parentKey">The Guid key of the parent node, or <c>null</c> for the root of the tree.</param>',
    '<param name="skip">The number of items to skip.</param>',
    '<param name="take">The maximum number of items to return.</param>',
    '<param name="propertyAliases">',
    "    Optional array of property aliases to load. If <c>null</c>, all properties are loaded.",
    "    If empty, no custom properties are loaded (only system properties).",
    "</param>",
    '<param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>A paged result containing the matching children.</returns>",
))

get_descendants = with_doc(generic(C["get_descendants"]), "GetDescendantsAsync(Guid ancestorKey,", doc(
    "<summary>",
    "    Gets a paged list of all descendants of a content node.",
    "</summary>",
    '<param name="ancestorKey">The Guid key of the ancestor node.</param>',
    '<param name="skip">The number of items to skip.</param>',
    '<param name="take">The maximum number of items to return.</param>',
    '<param name="ordering">The ordering specification, or <c>null</c> to order by path (ancestors before their own descendants).</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    '<param name="includeTrashed">Whether to include descendants that are currently in the recycle bin. Default is <c>true</c>.</param>',
    "<returns>A paged result containing the matching descendants.</returns>",
))

get_parent_by_key = generic(C["get_parent_by_key"])
get_parent_by_key = sub(get_parent_by_key, "content.ParentId == Constants.System.RecycleBinContent", "content.ParentId == RecycleBinId")
get_parent_by_key = with_doc(get_parent_by_key, "GetParentAsync(Guid key,", doc(
    "<summary>",
    "    Gets the parent of a content item.",
    "</summary>",
    '<param name="key">The Guid key of the content item.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The parent content item, or <c>null</c> if not found.</returns>",
))

get_root = with_doc(generic(C["get_root"]), "GetRootContentAsync(CancellationToken", doc(
    "<summary>",
    "    Gets root-level content items.",
    "</summary>",
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>The root-level content items.</returns>",
))

paged_recycle_bin = generic(C["paged_recycle_bin"])
paged_recycle_bin = sub(paged_recycle_bin, "GetPagedContentInRecycleBinAsync(", "GetPagedRecycleBinAsync(")
paged_recycle_bin = with_doc(paged_recycle_bin, "GetPagedRecycleBinAsync(int skip,", doc(
    "<summary>",
    "    Gets a paged list of content items in the recycle bin.",
    "</summary>",
    '<param name="skip">The number of items to skip.</param>',
    '<param name="take">The maximum number of items to return.</param>',
    '<param name="ordering">The ordering specification, or <c>null</c> to order by path.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>A paged result containing the matching recycle bin items.</returns>",
))

move_to_recycle_bin = generic(C["move_to_recycle_bin"])
move_to_recycle_bin = sub(move_to_recycle_bin, "var movingToRecycleBinNotification = new ContentMovingToRecycleBinNotification(moveEventInfo, eventMessages);", "MovingToRecycleBinNotification<TContent> movingToRecycleBinNotification = CreateMovingToRecycleBinNotification(moveEventInfo, eventMessages);")
move_to_recycle_bin = sub(move_to_recycle_bin, "await PerformMoveLockedAsync(content, Constants.System.RecycleBinContent, null, userId, moves, true, cancellationToken);", "await PerformMoveLockedAsync(content, RecycleBinId, null, userId, moves, true, cancellationToken);")
move_to_recycle_bin = sub(move_to_recycle_bin, "new ContentMovedToRecycleBinNotification(moveInfo, eventMessages).WithStateFrom(", "CreateMovedToRecycleBinNotification(moveInfo, eventMessages).WithStateFrom(")
move_to_recycle_bin = with_doc(move_to_recycle_bin, "MoveToRecycleBinAsync(TContent content, Guid userKey,", doc(
    "<summary>",
    "    Moves a content item to the recycle bin.",
    "</summary>",
    '<param name="content">The content item to move to the recycle bin.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>An attempt carrying the operation status.</returns>",
))

move = generic(C["move"])
move = sub(move, "if (parentKey == Constants.System.RecycleBinContentKey)", "if (parentKey == RecycleBinKey)")
move = sub(move, "if (content.ParentId == Constants.System.RecycleBinContent)", "if (content.ParentId == RecycleBinId)")
move = sub(move, "var movingNotification = new ContentMovingNotification(moveEventInfo, eventMessages);", "MovingNotification<TContent> movingNotification = CreateMovingNotification(moveEventInfo, eventMessages);")
move = sub(
    move,
    "        // if the content was trashed under another content, and so has a published version," + NL
    + "        // it cannot move back as published but has to be unpublished first - that's for the" + NL
    + "        // root content, everything underneath will retain its published status" + NL
    + "        if (content.Trashed && content.Published)" + NL
    + "        {" + NL
    + "            // however, it had been masked when being trashed, so there's no need for" + NL
    + "            // any special event here - just change its state" + NL
    + "            content.PublishedState = PublishedState.Unpublishing;" + NL
    + "        }" + NL,
    "        if (content.Trashed)" + NL
    + "        {" + NL
    + "            OnRestoringFromRecycleBin(content);" + NL
    + "        }" + NL,
)
move = sub(move, ".Where(x => x.ParentId == Constants.System.RecycleBinContent)];", ".Where(x => x.ParentId == RecycleBinId)];")
move = sub(move, "new ContentMovedNotification(moveInfo, eventMessages).WithStateFrom(movingNotification)", "CreateMovedNotification(moveInfo, eventMessages).WithStateFrom(movingNotification)")
move = with_doc(move, "MoveAsync(TContent content, Guid? parentKey, bool includeDescendants,", doc(
    "<summary>",
    "    Moves a content item under a new parent, optionally leaving its descendants behind.",
    "</summary>",
    '<param name="content">The content item to move.</param>',
    '<param name="parentKey">The Guid key of the new parent, or <c>null</c> for the root of the tree.</param>',
    '<param name="includeDescendants">',
    "    Whether to move the descendants of the content item along with it. When restoring an item out of the recycle",
    "    bin this can be set to <c>false</c> to restore only the item itself, leaving its descendants in the",
    "    recycle bin as top-level bin items.",
    "</param>",
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>An attempt carrying the operation status.</returns>",
))

perform_move_locked = generic(C["perform_move_locked"])
perform_move_locked = sub(perform_move_locked, "    // MUST be called from within a write lock on Constants.Locks.ContentTree.", "    // MUST be called from within a write lock on WriteLockIds.")
perform_move_locked = sub(perform_move_locked, "    private async Task PerformMoveLockedAsync(", "    protected async Task PerformMoveLockedAsync(")
perform_move_locked = sub(
    perform_move_locked,
    "            Constants.System.RecycleBinContent => Constants.System.RecycleBinContentKey," + NL,
    "            _ when parentId == RecycleBinId => RecycleBinKey," + NL,
)
perform_move_locked = sub(
    perform_move_locked,
    '                ? parentId == Constants.System.RecycleBinContent ? "-1,-20" : Constants.System.RootString',
    "                ? parentId == RecycleBinId ? RecycleBinPath : Constants.System.RootString",
)
perform_move_locked = sub(
    perform_move_locked,
    "            && parentId != Constants.System.RecycleBinContent" + NL
    + "            && originalPath.Contains(Constants.System.RecycleBinContentString);",
    "            && parentId != RecycleBinId" + NL
    + "            && originalPath.Contains(RecycleBinId.ToInvariantString());",
)

leave_descendant = generic(C["leave_descendant"])
leave_descendant = sub(leave_descendant, "? Constants.System.RecycleBinContentPathPrefix + descendant.Id", '? RecycleBinPath + "," + descendant.Id')
leave_descendant = sub(leave_descendant, "descendant.ParentId = Constants.System.RecycleBinContent;", "descendant.ParentId = RecycleBinId;")
leave_descendant = sub(leave_descendant, "descendant.ParentKey = Constants.System.RecycleBinContentKey;", "descendant.ParentKey = RecycleBinKey;")

perform_move_descendant = generic(C["perform_move_descendant"])
perform_move_content = generic(C["perform_move_content"])

empty_recycle_bin = generic(C["empty_recycle_bin"])
empty_recycle_bin = sub(
    empty_recycle_bin,
    "    /// <summary>" + NL
    + '    /// Empties the Recycle Bin by deleting all <see cref="TContent"/> items that reside in the bin asynchronously.' + NL
    + "    /// </summary>" + NL
    + '    /// <param name="userKey">The unique key of the user performing the operation.</param>' + NL
    + '    /// <param name="cancellationToken">The cancellation token.</param>' + NL
    + "    /// <returns>An attempt carrying the operation status.</returns>" + NL
    + "    public async Task<Attempt<ContentEmptyRecycleBinOperationStatus>> EmptyRecycleBinAsync(Guid userKey, CancellationToken cancellationToken)" + NL
    + "    {" + NL
    + "        var deleted = new List<TContent>();" + NL,
    "    /// <summary>" + NL
    + "    ///     Empties the recycle bin by deleting every item directly under it, with its descendants." + NL
    + "    /// </summary>" + NL
    + "    /// <remarks>" + NL
    + "    ///     Kept protected and exposed per kind: a kind whose recycle bin also holds things that are not its content" + NL
    + "    ///     (elements share theirs with containers) empties it elsewhere." + NL
    + "    /// </remarks>" + NL
    + '    /// <param name="userKey">The unique key of the user performing the operation.</param>' + NL
    + '    /// <param name="cancellationToken">The cancellation token.</param>' + NL
    + "    /// <returns>An attempt carrying the operation status.</returns>" + NL
    + "    protected async Task<Attempt<ContentEmptyRecycleBinOperationStatus>> EmptyRecycleBinCoreAsync(Guid userKey, CancellationToken cancellationToken)" + NL
    + "    {" + NL,
)
empty_recycle_bin = sub(empty_recycle_bin, "await GetChildrenAsync(Constants.System.RecycleBinContentKey, 0, int.MaxValue,", "await GetChildrenAsync(RecycleBinKey, 0, int.MaxValue,")
empty_recycle_bin = sub(empty_recycle_bin, "var emptyingRecycleBinNotification = new ContentEmptyingRecycleBinNotification(contents, eventMessages);", "EmptyingRecycleBinNotification<TContent> emptyingRecycleBinNotification = CreateEmptyingRecycleBinNotification(contents, eventMessages);")
empty_recycle_bin = sub(empty_recycle_bin, "var deletingContentNotification = new ContentDeletingNotification(contents, eventMessages);", "DeletingNotification<TContent> deletingContentNotification = CreateDeletingNotification(contents, eventMessages);")
RELATION_BLOCK = (
    '        // When checking if an item is related, we need to exclude the "relate parent on delete" relation type,' + NL
    + "        // as this is automatically created when items are trashed and would prevent emptying the recycle bin." + NL
    + "        int[]? relateParentOnDeleteRelationTypeIds = null;" + NL
    + "        if (_contentSettings.DisableDeleteWhenReferenced)" + NL
    + "        {" + NL
    + "            IRelationType? relateParentOnDeleteRelationType = await _relationService" + NL
    + "                .GetRelationTypeByAliasAsync(Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias);" + NL
    + "            if (relateParentOnDeleteRelationType is not null)" + NL
    + "            {" + NL
    + "                relateParentOnDeleteRelationTypeIds = [relateParentOnDeleteRelationType.Id];" + NL
    + "            }" + NL
    + "        }" + NL
    + NL
    + "        foreach (TContent content in contents)" + NL
    + "        {" + NL
    + "            if (_contentSettings.DisableDeleteWhenReferenced" + NL
    + "                && await _relationService.IsRelatedAsync(content.Id, RelationDirectionFilter.Child, excludeRelationTypeIds: relateParentOnDeleteRelationTypeIds))" + NL
    + "            {" + NL
    + "                continue;" + NL
    + "            }" + NL
    + NL
    + "            await DeleteLockedAsync(scope, content, eventMessages, cancellationToken);" + NL
    + "            deleted.Add(content);" + NL
    + "        }" + NL
)
empty_recycle_bin = sub(empty_recycle_bin, RELATION_BLOCK, "        List<TContent> deleted = await DeleteRecycleBinContentsLockedAsync(scope, contents, eventMessages, cancellationToken);" + NL)
empty_recycle_bin = sub(empty_recycle_bin, "new ContentEmptiedRecycleBinNotification(deleted, eventMessages).WithStateFrom(", "CreateEmptiedRecycleBinNotification(deleted, eventMessages).WithStateFrom(")
empty_recycle_bin = sub(empty_recycle_bin, 'await AuditAsync(AuditType.Delete, intUserId, Constants.System.RecycleBinContent, "Recycle bin emptied");', 'await AuditAsync(AuditType.Delete, intUserId, RecycleBinId, "Recycle bin emptied");')

sort = generic(C["sort"])
sort = sub(
    sort,
    "        var sortingNotification = new ContentSortingNotification(itemsA, eventMessages);" + NL
    + "        if (await scope.Notifications.PublishCancelableAsync(sortingNotification))" + NL,
    "        SortingNotification<TContent>? sortingNotification = CreateSortingNotification(itemsA, eventMessages);" + NL
    + "        if (sortingNotification is not null && await scope.Notifications.PublishCancelableAsync(sortingNotification))" + NL,
)
sort = sub(sort, "        var savingNotification = new ContentSavingNotification(itemsA, eventMessages);", "        SavingNotification<TContent> savingNotification = CreateSavingNotification(itemsA, eventMessages, isSorting: true);")
sort = sub(sort, "        var published = new List<TContent>();" + NL, "")
sort = sub(
    sort,
    "            // if it's published, register it, no point running StrategyPublish" + NL
    + "            // since we're not really publishing it and it cannot be cancelled etc" + NL
    + "            if (content.Published)" + NL
    + "            {" + NL
    + "                published.Add(content);" + NL
    + "            }" + NL
    + NL,
    "",
)
sort = sub(
    sort,
    "        scope.Notifications.Publish(" + NL
    + "            new ContentSavedNotification(itemsA, eventMessages).WithStateFrom(savingNotification));" + NL
    + "        scope.Notifications.Publish(" + NL
    + "            new ContentSortedNotification(itemsA, eventMessages).WithStateFrom(sortingNotification));" + NL,
    "        scope.Notifications.Publish(" + NL
    + "            CreateSavedNotification(itemsA, eventMessages).WithStateFrom(savingNotification));" + NL
    + "        SortedNotification<TContent>? sortedNotification = CreateSortedNotification(itemsA, eventMessages);" + NL
    + "        if (sortedNotification is not null)" + NL
    + "        {" + NL
    + "            scope.Notifications.Publish(sortingNotification is null ? sortedNotification : sortedNotification.WithStateFrom(sortingNotification));" + NL
    + "        }" + NL,
)
sort = sub(
    sort,
    "        if (published.Count > 0)" + NL
    + "        {" + NL
    + "            scope.Notifications.Publish(new ContentPublishedNotification(published, eventMessages));" + NL
    + "        }" + NL,
    "        OnSortedLocked(scope, saved, eventMessages);" + NL,
)
sort = with_doc(sort, "SortAsync(IReadOnlyList<Guid> orderedKeys,", doc(
    "<summary>",
    "    Sorts content items by persisting the supplied (already ordered) keys as the new sort order,",
    "    firing cancelable sorting/saving notifications and per-item saved/sorted/audit records for",
    "    every item whose position actually changes.",
    "</summary>",
    '<param name="orderedKeys">The Guid keys of the content items, in the desired order.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>An attempt carrying the operation status.</returns>",
    "<remarks>",
    '    Unlike <see cref="SortChildrenAsync" />, this reloads each item, fires cancelable',
    "    sorting/saving notifications, and only persists/audits items whose sort position changes.",
    "</remarks>",
))

sort_children = with_doc(generic(C["sort_children"]), "SortChildrenAsync(Guid? parentKey,", doc(
    "<summary>",
    "    Sorts the children of a parent by persisting the supplied (already ordered) child keys as the",
    "    new sort order, in a single batched operation.",
    "</summary>",
    '<param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the tree.</param>',
    '<param name="orderedChildKeys">The Guid keys of the children, in the desired order.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<returns>An attempt carrying the operation status.</returns>",
    "<remarks>",
    '    Unlike <see cref="SortAsync" />, this does not load the children or fire per-item',
    "    save/sort notifications; it persists the order directly and refreshes the affected cache branch.",
    "</remarks>",
))

delete_of_type = with_doc(C["delete_of_type"], "DeleteOfTypeAsync(Guid contentTypeKey,", doc(
    "<summary>",
    "    Deletes all content of a given type.",
    "</summary>",
    '<param name="contentTypeKey">The Guid key of the content type.</param>',
    '<param name="userKey">The Guid key of the user performing the action.</param>',
    '<param name="cancellationToken">The cancellation token.</param>',
    "<remarks>",
    "    <para>All non-deleted descendants of the deleted content are moved to the recycle bin.</para>",
    "    <para>This operation is potentially dangerous and expensive.</para>",
    "</remarks>",
    "<returns>An attempt carrying the operation status.</returns>",
))

# ---------------------------------------------------------------------------------------------------------
# The wide base.
# ---------------------------------------------------------------------------------------------------------
HEADER = """using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Base class for services that manage one kind of content through an
///     <see cref="IAsyncContentRepository{TEntity}" />.
/// </summary>
/// <typeparam name="TContent">The type of content managed by the concrete implementation.</typeparam>
/// <typeparam name="TContentType">The type of content type the content is built from.</typeparam>
/// <remarks>
///     Implements the shared lookup, count, version, save, delete, create, move, recycle bin and sort behaviour, leaving
///     the kind-specific parts (object type, lock ids, notifications, entity construction) to the derived service.
///     Publishing is layered on top by <see cref="PublishableContentServiceBase{TContent}" />.
/// </remarks>
public abstract class ContentServiceBase<TContent, TContentType> : RepositoryService, IAsyncContentServiceBase<TContent>
    where TContent : class, IContentBase
    where TContentType : class, IContentTypeComposition
{
    private readonly IAuditService _auditService;
    private readonly IAsyncContentTypeRepositoryBase<TContentType> _contentTypeRepository;
    private readonly IAsyncContentRepository<TContent> _contentRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentServiceBase{TContent, TContentType}" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="contentTypeRepository">The backing content type repository.</param>
    /// <param name="contentRepository">The backing content repository.</param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="idKeyMap">The ID key map.</param>
    protected ContentServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        IAsyncContentTypeRepositoryBase<TContentType> contentTypeRepository,
        IAsyncContentRepository<TContent> contentRepository,
        ILanguageRepository languageRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IIdKeyMap idKeyMap)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _auditService = auditService;
        _contentTypeRepository = contentTypeRepository;
        _contentRepository = contentRepository;
        _languageRepository = languageRepository;
        _userIdKeyResolver = userIdKeyResolver;
        _idKeyMap = idKeyMap;
    }
""".replace("\n", NL)

NEW_HOOKS = """    /// <summary>
    ///     Gets the Guid key of the recycle bin node for this kind of content.
    /// </summary>
    protected Guid RecycleBinKey => _contentRepository.RecycleBinKey;

    // The path of the recycle bin node itself ("-1,<bin id>"), the prefix of every trashed item's path.
    private string RecycleBinPath => Constants.System.RootString + "," + RecycleBinId.ToInvariantString();

    /// <summary>
    ///     Creates a new, unsaved content item of this kind.
    /// </summary>
    /// <param name="name">The name of the content item.</param>
    /// <param name="parent">The parent content item, or <c>null</c> for the root of the tree.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="userId">The identifier of the user creating the content item.</param>
    /// <returns>The new content item.</returns>
    protected abstract TContent CreateEntity(string name, TContent? parent, TContentType contentType, int userId);

    protected abstract DeletingNotification<TContent> CreateDeletingNotification(IEnumerable<TContent> content, EventMessages eventMessages);

    protected abstract MovingNotification<TContent> CreateMovingNotification(MoveEventInfo<TContent> moveInfo, EventMessages eventMessages);

    protected abstract MovedNotification<TContent> CreateMovedNotification(IEnumerable<MoveEventInfo<TContent>> moveInfos, EventMessages eventMessages);

    protected abstract MovingToRecycleBinNotification<TContent> CreateMovingToRecycleBinNotification(MoveToRecycleBinEventInfo<TContent> moveInfo, EventMessages eventMessages);

    protected abstract MovedToRecycleBinNotification<TContent> CreateMovedToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<TContent>> moveInfos, EventMessages eventMessages);

    protected abstract EmptyingRecycleBinNotification<TContent> CreateEmptyingRecycleBinNotification(IEnumerable<TContent> content, EventMessages eventMessages);

    protected abstract EmptiedRecycleBinNotification<TContent> CreateEmptiedRecycleBinNotification(IEnumerable<TContent> content, EventMessages eventMessages);

    /// <summary>
    ///     Creates the cancelable notification published before content items are sorted, or <c>null</c> when this
    ///     kind has no such notification.
    /// </summary>
    /// <param name="content">The items being sorted.</param>
    /// <param name="eventMessages">The event messages collection.</param>
    /// <returns>The notification to publish, or <c>null</c>.</returns>
    protected virtual SortingNotification<TContent>? CreateSortingNotification(IEnumerable<TContent> content, EventMessages eventMessages) => null;

    /// <summary>
    ///     Creates the notification published after content items have been sorted, or <c>null</c> when this kind
    ///     has no such notification.
    /// </summary>
    /// <param name="content">The items that were sorted.</param>
    /// <param name="eventMessages">The event messages collection.</param>
    /// <returns>The notification to publish, or <c>null</c>.</returns>
    protected virtual SortedNotification<TContent>? CreateSortedNotification(IEnumerable<TContent> content, EventMessages eventMessages) => null;

    /// <summary>
    ///     Creates the "saving" notification for multiple items, saying whether the save is part of a sort.
    /// </summary>
    /// <remarks>
    ///     Defaults to the plain <see cref="CreateSavingNotification(IEnumerable{TContent}, EventMessages)" /> overload;
    ///     a kind whose saving notification carries the sorting flag overrides this to set it.
    /// </remarks>
    /// <param name="content">The items being saved.</param>
    /// <param name="eventMessages">The event messages collection.</param>
    /// <param name="isSorting">Whether the items are saved as part of a sort.</param>
    /// <returns>The notification to publish.</returns>
    protected virtual SavingNotification<TContent> CreateSavingNotification(IEnumerable<TContent> content, EventMessages eventMessages, bool isSorting)
        => CreateSavingNotification(content, eventMessages);

    /// <summary>
    ///     Validates a content item before it is saved, beyond the checks every kind shares.
    /// </summary>
    /// <param name="content">The content item about to be saved.</param>
    /// <returns>
    ///     <see cref="ContentSaveOperationStatus.Success" /> when the item may be saved, otherwise the status the save
    ///     is rejected with.
    /// </returns>
    protected virtual ContentSaveOperationStatus ValidateForSave(TContent content) => ContentSaveOperationStatus.Success;

    /// <summary>
    ///     Called inside the delete scope, after the deleting notification has passed and before the item is deleted.
    /// </summary>
    /// <param name="scope">The scope the delete runs in.</param>
    /// <param name="content">The content item about to be deleted.</param>
    /// <param name="eventMessages">The event messages collection.</param>
    protected virtual void BeforeDeleteLocked(ICoreScope scope, TContent content, EventMessages eventMessages)
    {
    }

    /// <summary>
    ///     Called when a trashed content item is about to be moved out of the recycle bin, before it is persisted.
    /// </summary>
    /// <param name="content">The content item being restored.</param>
    protected virtual void OnRestoringFromRecycleBin(TContent content)
    {
    }

    /// <summary>
    ///     Called inside the sort scope once the items whose position changed have been saved and the saved, sorted
    ///     and tree change notifications have been published.
    /// </summary>
    /// <param name="scope">The scope the sort runs in.</param>
    /// <param name="sorted">The items whose sort order changed, in their new order.</param>
    /// <param name="eventMessages">The event messages collection.</param>
    protected virtual void OnSortedLocked(ICoreScope scope, IReadOnlyList<TContent> sorted, EventMessages eventMessages)
    {
    }

    /// <summary>
    ///     Gets a value indicating whether a version of a content item may be deleted.
    /// </summary>
    /// <param name="content">The content item, at its current version.</param>
    /// <param name="versionId">The id of the version to delete.</param>
    /// <returns><c>true</c> when the version is not one the kind must keep; otherwise, <c>false</c>.</returns>
    protected virtual bool IsVersionDeletable(TContent content, int versionId) => content.VersionId != versionId;

    /// <summary>
    ///     Deletes the items found in the recycle bin, inside the scope that empties it, and returns the ones deleted.
    /// </summary>
    /// <param name="scope">The scope the operation runs in.</param>
    /// <param name="contents">The items directly under the recycle bin.</param>
    /// <param name="eventMessages">The event messages collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The items that were deleted.</returns>
    protected virtual async Task<List<TContent>> DeleteRecycleBinContentsLockedAsync(ICoreScope scope, IReadOnlyList<TContent> contents, EventMessages eventMessages, CancellationToken cancellationToken)
    {
        var deleted = new List<TContent>();
        foreach (TContent content in contents)
        {
            await DeleteLockedAsync(scope, content, eventMessages, cancellationToken);
            deleted.Add(content);
        }

        return deleted;
    }
""".replace("\n", NL)


def region(name: str, *members: str) -> str:
    return NL.join([f"    #region {name}", "", (NL + NL).join(members), "", "    #endregion"])


wide = NL.join([
    HEADER,
    P["hooks_object"],
    "",
    hook_logger,
    "",
    P["hook_delete_locked"],
    "",
    P["hooks_saving_saved"],
    "",
    P["hook_tree_change_single"],
    "",
    P["hook_tree_change_many"],
    "",
    P["hook_deleting"],
    "",
    P["hooks_versions"],
    "",
    P["hooks_saved_cultures"],
    "",
    P["build_culture_map"],
    "",
    NEW_HOOKS,
    region("Create", create, throw_if_save_rejected),
    "",
    region("Count", P["count"]),
    "",
    region("Get, Has, Is", P["get_by_id"], get_by_ids, get_by_ids_aliases, paged_of_type, paged_of_types, versions, P["has_children"], P["get_parent"], get_children, get_descendants, get_parent_by_key, get_root, paged_recycle_bin),
    "",
    region("Save", save_single, P["save_many"]),
    "",
    region("Delete", delete, delete_versions_doc := P["delete_versions"], delete_version),
    "",
    region("Move, RecycleBin", move_to_recycle_bin, move, perform_move_locked, leave_descendant, perform_move_descendant, perform_move_content, empty_recycle_bin),
    "",
    region("Sort", sort, sort_children),
    "",
    region("Others", P["has_unsaved_changes"], P["check_data_integrity"]),
    "",
    region("Auditing", P["audit"]),
    "",
    region("Content Types", delete_of_types, delete_of_type, get_content_type),
    "}",
    "",
])
WIDE.write_text(wide, encoding="utf-8", newline="")


# ---------------------------------------------------------------------------------------------------------
# Remove the hoisted ranges from the two origin files; the remaining edits are done by hand.
# ---------------------------------------------------------------------------------------------------------
def remove(lines: list[str], ranges: list[tuple[int, int]]) -> list[str]:
    removed = set()
    for start, end in ranges:
        for i in range(start, end + 1):
            assert i not in removed, f"line {i} removed twice"
            removed.add(i)
    kept = [line for i, line in enumerate(lines, start=1) if i not in removed]
    collapsed: list[str] = []
    for line in kept:
        if line == "" and collapsed and collapsed[-1] == "":
            continue
        collapsed.append(line)
    return collapsed


PCSB.write_text(NL.join(remove(pcsb, P_REMOVED)) + NL, encoding="utf-8", newline="")
CS.write_text(NL.join(remove(cs, C_REMOVED)) + NL, encoding="utf-8", newline="")
print("wide base:", wide.count(NL) + 1, "lines")
