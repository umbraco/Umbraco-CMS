import type { UmbAuditLogModel, UmbAuditLogRequestArgs, UmbAuditLogTagData } from './types.js';
import type { UmbPagedModel, UmbRepositoryBase, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbAuditLogRepository<AuditLogType extends UmbAuditLogModel<any> = UmbAuditLogModel> extends UmbRepositoryBase {
	requestAuditLog(args: UmbAuditLogRequestArgs): Promise<UmbRepositoryResponse<UmbPagedModel<AuditLogType>>>;
	getTagStyleAndText?(logType: string): UmbAuditLogTagData;
}
