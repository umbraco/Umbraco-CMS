import type { UmbAuditLogTagData } from './info-app/types.js';
import type { UmbAuditLogModel, UmbAuditLogRepository } from '@umbraco-cms/backoffice/audit-log';

export interface UmbAuditLogHistoryRepository<AuditLogType extends UmbAuditLogModel = UmbAuditLogModel>
	extends UmbAuditLogRepository<AuditLogType> {
	getTagStyleAndText(logType: string): UmbAuditLogTagData;
}
