import type { UmbAuditLogType } from '@umbraco-cms/backoffice/audit-log';

export type UmbElementAuditLogType =
	| 'New'
	| 'Save'
	| 'SaveVariant'
	| 'Open'
	| 'Delete'
	| 'Publish'
	| 'PublishVariant'
	| 'Unpublish'
	| 'UnpublishVariant'
	| 'Move'
	| 'Copy'
	| 'Sort'
	| 'RollBack'
	| 'ContentVersionPreventCleanup'
	| 'ContentVersionEnableCleanup'
	| UmbAuditLogType;

export const UmbElementAuditLog = Object.freeze({
	CONTENT_VERSION_ENABLE_CLEANUP: 'ContentVersionEnableCleanup',
	CONTENT_VERSION_PREVENT_CLEANUP: 'ContentVersionPreventCleanup',
	COPY: 'Copy',
	CUSTOM: 'Custom',
	DELETE: 'Delete',
	MOVE: 'Move',
	NEW: 'New',
	OPEN: 'Open',
	PUBLISH_VARIANT: 'PublishVariant',
	PUBLISH: 'Publish',
	ROLL_BACK: 'RollBack',
	SAVE_VARIANT: 'SaveVariant',
	SAVE: 'Save',
	SORT: 'Sort',
	SYSTEM: 'System',
	UNPUBLISH_VARIANT: 'UnpublishVariant',
	UNPUBLISH: 'Unpublish',
});
