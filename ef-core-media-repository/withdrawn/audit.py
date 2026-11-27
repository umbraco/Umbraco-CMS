"""Normalised diff of every member hoisted into ContentServiceBase against its origin at HEAD.

Origin bodies are retyped with the substitutions a hoist is allowed to make (type parameters, lock ids, repository
field, notification factories, recycle bin constants); whatever still differs is a deliberate hook edit and is
printed for review.
"""
import difflib
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(sys.argv[1])
WIDE = ROOT / "src" / "Umbraco.Core" / "Services" / "ContentServiceBase.cs"


def origin(path: str) -> list[str]:
    text = subprocess.run(["git", "-C", str(ROOT), "show", f"HEAD:{path}"], capture_output=True, check=True).stdout.decode("utf-8")
    return text.replace("\r\n", "\n").split("\n")


def member_from(lines: list[str], signature: str) -> list[str]:
    """The member whose declaration line contains `signature`, from that line to its closing brace or `;`."""
    hits = [i for i, line in enumerate(lines) if signature in line and not line.strip().startswith("///")]
    assert len(hits) == 1, f"{signature!r}: {len(hits)} hits"
    start = hits[0]
    depth = 0
    seen_brace = False
    for i in range(start, len(lines)):
        line = lines[i]
        depth += line.count("{") - line.count("}")
        if "{" in line:
            seen_brace = True
        if seen_brace and depth == 0:
            return lines[start:i + 1]
        if not seen_brace and line.rstrip().endswith(";"):
            return lines[start:i + 1]
    raise AssertionError(f"unterminated member {signature!r}")


def retype(text: str) -> str:
    text = re.sub(r"\bIContentType\b", "TContentType", text)
    text = re.sub(r"\bIContent\b", "TContent", text)
    text = text.replace("scope.WriteLock(Constants.Locks.ContentTree);", "scope.WriteLock(WriteLockIds);")
    text = text.replace("scope.ReadLock(Constants.Locks.ContentTree);", "scope.ReadLock(ReadLockIds);")
    text = text.replace("_documentRepository.", "_contentRepository.")
    text = text.replace("new ContentTreeChangeNotification(", "CreateTreeChangeNotification(")
    text = text.replace("Constants.System.RecycleBinContentKey", "RecycleBinKey")
    text = re.sub(r"Constants\.System\.RecycleBinContent\b", "RecycleBinId", text)
    text = text.replace("ILogger<PublishableContentServiceBase<TContent>>", "ILogger<ContentServiceBase<TContent, TContentType>>")
    return text


wide = WIDE.read_text(encoding="utf-8").replace("\r\n", "\n").split("\n")
pcsb = origin("src/Umbraco.Core/Services/PublishableContentServiceBase.cs")
cs = origin("src/Umbraco.Core/Services/ContentService.cs")

# (origin file, origin signature, new signature) - the signature is a fragment of the declaration line.
MEMBERS = [
    (pcsb, "protected abstract UmbracoObjectTypes ContentObjectType", None),
    (pcsb, "protected abstract int RecycleBinId", None),
    (pcsb, "protected abstract int[] ReadLockIds", None),
    (pcsb, "protected abstract int[] WriteLockIds", None),
    (pcsb, "Logger { get; }", None),
    (pcsb, "protected abstract Task DeleteLockedAsync(", None),
    (pcsb, "CreateSavingNotification(TContent content, EventMessages eventMessages);", None),
    (pcsb, "CreateSavedNotification(TContent content, EventMessages eventMessages);", None),
    (pcsb, "CreateSavingNotification(IEnumerable<TContent> content, EventMessages eventMessages);", None),
    (pcsb, "CreateSavedNotification(IEnumerable<TContent> content, EventMessages eventMessages);", None),
    (pcsb, "CreateTreeChangeNotification(TContent content, TreeChangeTypes changeTypes, EventMessages eventMessages);", None),
    (pcsb, "CreateTreeChangeNotification(IEnumerable<TContent> content, TreeChangeTypes changeTypes, EventMessages eventMessages);", None),
    (pcsb, "CreateDeletingNotification(TContent content, EventMessages eventMessages);", None),
    (pcsb, "CreateDeletingVersionsNotification(Guid key,", None),
    (pcsb, "CreateDeletedVersionsNotification(Guid key,", None),
    (pcsb, "CreateSavedNotification(TContent content, EventMessages eventMessages, IReadOnlyDictionary", None),
    (pcsb, "CreateSavedNotification(IEnumerable<TContent> content, EventMessages eventMessages, IReadOnlyDictionary", None),
    (pcsb, "BuildCultureMap(TContent content,", None),
    (pcsb, "public async Task<int> CountAsync(", None),
    (pcsb, "public async Task<int> CountChildrenAsync(", None),
    (pcsb, "public async Task<int> CountDescendantsAsync(", None),
    (pcsb, "GetByIdAsync(Guid key, CancellationToken", None),
    (pcsb, "GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken", None),
    (pcsb, "GetPagedOfTypeAsync(Guid contentTypeKey,", None),
    (pcsb, "GetPagedOfTypesAsync(Guid[] contentTypeKeys,", None),
    (pcsb, "GetVersionAsync(int versionId,", None),
    (pcsb, "GetVersionsAsync(Guid contentKey,", None),
    (pcsb, "GetVersionsSlimAsync(Guid contentKey,", None),
    (pcsb, "GetVersionIdsAsync(Guid contentKey,", None),
    (pcsb, "HasChildrenAsync(Guid key,", None),
    (pcsb, "GetParentAsync(TContent? content,", None),
    (pcsb, "SaveAsync(TContent content, Guid userKey, ContentScheduleCollection? contentSchedule,", "PerformSaveAsync(TContent content, Guid userKey, Func<Task>? persistAlongside,"),
    (pcsb, "SaveAsync(IEnumerable<TContent> contents,", None),
    (pcsb, "DeleteAsync(TContent content, Guid userKey,", None),
    (pcsb, "DeleteVersionsAsync(Guid key, DateTime versionDate,", None),
    (pcsb, "DeleteVersionAsync(Guid key, int versionId,", None),
    (pcsb, "HasUnsavedChanges(TContent content)", None),
    (pcsb, "public abstract Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(", None),
    (pcsb, "protected async Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(", None),
    (pcsb, "protected async Task AuditAsync(", None),
    (pcsb, "protected static string GetLanguageDetailsForAuditEntry(", None),
    (pcsb, "public abstract Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypesAsync(", None),
    (pcsb, "GetContentTypeAsync(ICoreScope scope,", None),
    (cs, "CreateAsync(string name, Guid? parentKey, string contentTypeAlias,", "CreateCoreAsync(string name, Guid? parentKey, string contentTypeAlias,"),
    (cs, "CreateAsync(string name, Guid? parentKey, IContentType contentType,", "CreateCoreAsync(string name, Guid? parentKey, TContentType contentType,"),
    (cs, "CreateAsync(string name, IContent parent, string contentTypeAlias,", "CreateCoreAsync(string name, TContent parent, string contentTypeAlias,"),
    (cs, "CreateAndSaveAsync(string name, Guid? parentKey, string contentTypeAlias,", "CreateAndSaveCoreAsync(string name, Guid? parentKey, string contentTypeAlias,"),
    (cs, "CreateAndSaveAsync(string name, IContent parent, string contentTypeAlias,", "CreateAndSaveCoreAsync(string name, TContent parent, string contentTypeAlias,"),
    (cs, "private static void ThrowIfSaveRejected(", "protected virtual void ThrowIfSaveRejected("),
    (cs, "GetChildrenAsync(Guid? parentKey, int skip, int take, string[]? propertyAliases,", None),
    (cs, "GetDescendantsAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true)", None),
    (cs, "GetParentAsync(Guid key,", None),
    (cs, "GetRootContentAsync(CancellationToken", None),
    (cs, "GetPagedContentInRecycleBinAsync(int skip,", "GetPagedRecycleBinAsync(int skip,"),
    (cs, "MoveToRecycleBinAsync(IContent content, Guid userKey,", "MoveToRecycleBinAsync(TContent content, Guid userKey,"),
    (cs, "MoveAsync(IContent content, Guid? parentKey, bool includeDescendants,", "MoveCoreAsync(TContent content, Guid? parentKey, bool includeDescendants,"),
    (cs, "private async Task PerformMoveLockedAsync(", "protected async Task PerformMoveLockedAsync("),
    (cs, "private async Task LeaveDescendantInRecycleBinLockedAsync(", None),
    (cs, "private async Task PerformMoveDescendantLockedAsync(", None),
    (cs, "private async Task PerformMoveContentLockedAsync(", None),
    (cs, "EmptyRecycleBinAsync(Guid userKey,", "EmptyRecycleBinCoreAsync(Guid userKey,"),
    (cs, "SortAsync(IReadOnlyList<Guid> orderedKeys,", None),
    (cs, "SortChildrenAsync(Guid? parentKey,", "SortChildrenCoreAsync(Guid? parentKey,"),
    (cs, "DeleteOfTypeAsync(Guid contentTypeKey,", None),
]

identical = 0
for source, old_signature, new_signature in MEMBERS:
    before = retype("\n".join(member_from(source, old_signature))).split("\n")
    after = member_from(wide, new_signature or retype(old_signature))
    if before == after:
        identical += 1
        continue
    print(f"=== {new_signature or old_signature}")
    for line in difflib.unified_diff(before, after, "origin", "wide", n=1, lineterm=""):
        if line.startswith(("---", "+++", "@@")):
            continue
        print(line)
    print()

print(f"{identical} of {len(MEMBERS)} members identical after retyping; the rest are printed above.")
