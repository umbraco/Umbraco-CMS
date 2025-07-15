import type { UmbAuditLogModel, UmbAuditLogRequestArgs } from './types.js';
import type { UmbPagedModel, UmbRepositoryBase, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbAuditLogRepository<AuditLogType extends UmbAuditLogModel<any>> extends UmbRepositoryBase {
	requestAuditLog(args: UmbAuditLogRequestArgs): Promise<UmbRepositoryResponse<UmbPagedModel<AuditLogType>>>;
}
