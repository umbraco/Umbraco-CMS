import type { UmbAuditLogType } from '@umbraco-cms/backoffice/audit-log';

export type UmbDocumentBlueprintAuditLogType = 'New' | 'Save' | 'SaveVariant' | 'Open' | 'Move' | UmbAuditLogType;

export const UmbDocumentBlueprintAuditLog = Object.freeze({
	CUSTOM: 'Custom',
	MOVE: 'Move',
	NEW: 'New',
	OPEN: 'Open',
	SAVE_VARIANT: 'SaveVariant',
	SAVE: 'Save',
	SYSTEM: 'System',
});
