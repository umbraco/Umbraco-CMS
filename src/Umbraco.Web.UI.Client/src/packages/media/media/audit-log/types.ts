import type { UmbMediaAuditLogType } from './utils/index.js';
import type { UmbAuditLogModel } from '@umbraco-cms/backoffice/audit-log';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaAuditLogModel extends UmbAuditLogModel<UmbMediaAuditLogType> {}
