import type { UmbAuditLogType } from '@umbraco-cms/backoffice/audit-log';

export type UmbDocumentAuditLogType =
	| 'New'
	| 'Save'
	| 'SaveVariant'
	| 'Open'
	| 'Delete'
	| 'Publish'
	| 'PublishVariant'
	| 'SendToPublish'
	| 'SendToPublishVariant'
	| 'Unpublish'
	| 'UnpublishVariant'
	| 'Move'
	| 'Copy'
	| 'AssignDomain'
	| 'PublicAccess'
	| 'Sort'
	| 'Notify'
	| 'RollBack'
	| 'ContentVersionPreventCleanup'
	| 'ContentVersionEnableCleanup'
	| UmbAuditLogType;

export const UmbDocumentAuditLog = Object.freeze({
	ASSIGN_DOMAIN: 'AssignDomain',
	CONTENT_VERSION_ENABLE_CLEANUP: 'ContentVersionEnableCleanup',
	CONTENT_VERSION_PREVENT_CLEANUP: 'ContentVersionPreventCleanup',
	COPY: 'Copy',
	CUSTOM: 'Custom',
	DELETE: 'Delete',
	MOVE: 'Move',
	NEW: 'New',
	NOTIFY: 'Notify',
	OPEN: 'Open',
	PUBLIC_ACCESS: 'PublicAccess',
	PUBLISH_VARIANT: 'PublishVariant',
	PUBLISH: 'Publish',
	ROLL_BACK: 'RollBack',
	SAVE_VARIANT: 'SaveVariant',
	SAVE: 'Save',
	SEND_TO_PUBLISH_VARIANT: 'SendToPublishVariant',
	SEND_TO_PUBLISH: 'SendToPublish',
	SORT: 'Sort',
	SYSTEM: 'System',
	UNPUBLISH_VARIANT: 'UnpublishVariant',
	UNPUBLISH: 'Unpublish',
});
