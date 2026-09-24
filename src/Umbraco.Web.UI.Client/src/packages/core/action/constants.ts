/*
 * Groups used by core actions (entity actions, property actions and workspace action menu items).
 * A separator is rendered between adjacent actions of different groups. Extensions can use these
 * to place an action in a core group, or set a group of their own.
 */

/** Actions that create something new, e.g. Create, Create Document Blueprint and Invite user. */
export const UMB_ACTION_GROUP_CREATE = 'create';

/**
 * Actions that remove something or manage the recycle bin, e.g. Trash, Delete, Empty recycle bin and Clear.
 * Restore from recycle bin is included, as it is only shown alongside Delete on trashed items.
 */
export const UMB_ACTION_GROUP_DELETE = 'delete';

/** Actions that change where an item sits or what it is called, e.g. Move to, Duplicate to, Sort and Rename. */
export const UMB_ACTION_GROUP_STRUCTURE = 'structure';

/** Actions that publish content or manage its versions, e.g. Publish, Unpublish, Schedule publish and Rollback. */
export const UMB_ACTION_GROUP_PUBLISHING = 'publishing';

/** Actions that take content offline, kept apart from publishing in the "Save and publish" menu, e.g. Unpublish. */
export const UMB_ACTION_GROUP_UNPUBLISHING = 'unpublishing';

/** Actions that configure an item, e.g. Culture and Hostnames, Public Access and Notifications. */
export const UMB_ACTION_GROUP_SETTINGS = 'settings';

/**
 * Actions that move content in or out of an installation or between environments, e.g. Import and Export.
 * Packages that deploy, sync or compare content between environments are encouraged to use this group.
 */
export const UMB_ACTION_GROUP_TRANSFER = 'transfer';

/** Actions that manage a user's account, e.g. Enable, Disable, Unlock, Change password and Configure MFA. */
export const UMB_ACTION_GROUP_USER = 'user';

/** Actions that copy to or paste from the clipboard. */
export const UMB_ACTION_GROUP_CLIPBOARD = 'clipboard';

/** Actions that act on the tree itself, e.g. Reload children. */
export const UMB_ACTION_GROUP_TREE = 'tree';
