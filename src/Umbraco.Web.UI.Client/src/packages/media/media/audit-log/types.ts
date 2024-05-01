import type { UmbAuditLogModel } from '@umbraco-cms/backoffice/audit-log';
import type { UmbMediaAuditLogType } from './utils/index.js';

export interface UmbMediaAuditLogModel extends UmbAuditLogModel<UmbMediaAuditLogType> {}
