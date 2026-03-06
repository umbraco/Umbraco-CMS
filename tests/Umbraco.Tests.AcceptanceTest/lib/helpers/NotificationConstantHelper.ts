export class NotificationConstantHelper {

  public static readonly success = {
    created: "Created",
    success: 'Success',
    saved: "Saved",
    published: "Document published",
    unpublished: "Document unpublished",
    copied: "Copied",
    moved: "Moved",
    movedToRecycleBin: "Trashed",
    deleted: "Deleted",
    emptiedRecycleBin: "Recycle Bin Emptied",
    restored: "Restored",
    duplicated: "Duplicated",
    renamed: "Renamed",
    folderCreated: "Folder created",
    folderUpdated: "Folder updated",
    folderDeleted: "Folder deleted",
    userDisabled: " is now disabled",
    userEnabled: " is now enabled",
    avatarUploaded: "Avatar uploaded",
    avatarDeleted: "Avatar deleted",
    documentBlueprintCreated: 'Document Blueprint created',
    culturesAndHostnamesSaved: 'Cultures and hostnames saved',
    publicAccessSettingCreated: 'Public access setting created',
    itemsSorted: 'Items sorted',
    passwordChanged: 'Password changed',
    schedulePublishingUpdated: 'A schedule for publishing has been updated',
    publishWithDescendants: 'and subpages have been published'
  }

  public static readonly error = {
    error: 'Error',
    emptyName: "Name was empty or null",
    duplicateName: "Duplicate name",
    invalidEmail: "Invalid email supplied",
    notEmptyFolder: "The folder is not empty",
    duplicateISOcode: "Duplicate ISO code",
    notEmpty: "Not empty",
    noAccessToResource: "The authenticated user do not have access to this resource",
    documentWasNotPublished: 'Document was not published, but we saved it for you.',
    documentCouldNotBePublished: 'Document could not be published, but we saved it for you',
    parentNotPublished: 'Parent not published',
    permissionDenied: 'Permission Denied'
  }
}