import type { UmbAuditLogModel, UmbAuditLogRequestArgs } from './types.js';
import type { UmbRepositoryBase, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbAuditLogRepository<AuditLogType extends UmbAuditLogModel> extends UmbRepositoryBase {
	requestAuditLog(args: UmbAuditLogRequestArgs): Promise<UmbRepositoryResponse<AuditLogType>>;
}
