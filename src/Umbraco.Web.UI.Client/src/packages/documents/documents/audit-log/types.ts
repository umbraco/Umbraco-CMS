import type { UmbDocumentAuditLogType } from './utils/index.js';
import type { UmbAuditLogModel } from '@umbraco-cms/backoffice/audit-log';

export interface UmbDocumentAuditLogModel extends UmbAuditLogModel<UmbDocumentAuditLogType> {}
