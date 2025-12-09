import { dataSet } from './sets/index.js';
import type { AuditLogResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const logs: Array<AuditLogResponseModel> = dataSet.auditLogs;
