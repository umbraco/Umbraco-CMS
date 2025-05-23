import type { UmbAuditLogModel, UmbAuditLogRequestArgs } from './types.js';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbAuditLogDataSource<AuditLogType extends UmbAuditLogModel> {
	getAuditLog(args: UmbAuditLogRequestArgs): Promise<UmbDataSourceResponse<UmbPagedModel<AuditLogType>>>;
}
