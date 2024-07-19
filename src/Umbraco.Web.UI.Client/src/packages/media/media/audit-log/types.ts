import type { UmbMediaAuditLogType } from './utils/index.js';
import type { UmbAuditLogModel } from '@umbraco-cms/backoffice/audit-log';

export interface UmbMediaAuditLogModel extends UmbAuditLogModel<UmbMediaAuditLogType> {}
