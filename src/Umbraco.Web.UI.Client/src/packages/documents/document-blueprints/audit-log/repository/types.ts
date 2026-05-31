import type { UmbAuditLogModel, UmbAuditLogType } from '@umbraco-cms/backoffice/audit-log';

export type UmbDocumentBlueprintAuditLogType = 'New' | 'Save' | 'SaveVariant' | 'Open' | 'Move' | UmbAuditLogType;

export type UmbDocumentBlueprintAuditLogModel = UmbAuditLogModel<UmbDocumentBlueprintAuditLogType>;
