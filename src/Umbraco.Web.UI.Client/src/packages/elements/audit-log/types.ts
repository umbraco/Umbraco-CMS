import type { UmbElementAuditLogType } from './utils/index.js';
import type { UmbAuditLogModel } from '@umbraco-cms/backoffice/audit-log';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementAuditLogModel extends UmbAuditLogModel<UmbElementAuditLogType> {}
