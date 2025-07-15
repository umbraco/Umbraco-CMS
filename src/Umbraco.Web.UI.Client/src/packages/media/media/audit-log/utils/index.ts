import type { UmbAuditLogType } from '@umbraco-cms/backoffice/audit-log';

export type UmbMediaAuditLogType = 'New' | 'Save' | 'Open' | 'Delete' | 'Move' | 'Copy' | 'Sort' | UmbAuditLogType;

export const UmbMediaAuditLog = Object.freeze({
	COPY: 'Copy',
	CUSTOM: 'Custom',
	DELETE: 'Delete',
	MOVE: 'Move',
	NEW: 'New',
	OPEN: 'Open',
	SAVE: 'Save',
	SORT: 'Sort',
	SYSTEM: 'System',
});
