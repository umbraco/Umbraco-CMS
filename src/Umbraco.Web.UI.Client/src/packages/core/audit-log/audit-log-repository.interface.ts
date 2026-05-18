import type { UmbAuditLogModel, UmbAuditLogRequestArgs, UmbAuditLogTagData } from './types.js';
import type { UmbPagedModel, UmbRepositoryBase, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbAuditLogRepository<AuditLogType extends UmbAuditLogModel<any> = UmbAuditLogModel> extends UmbRepositoryBase {
	requestAuditLog(args: UmbAuditLogRequestArgs): Promise<UmbRepositoryResponse<UmbPagedModel<AuditLogType>>>;

	/**
	 * Legacy fallback for styling built-in audit log types. Core ships `auditLogType` extension manifests
	 * for the standard set of audit types, and the audit log info element resolves against those manifests
	 * first; this method is only consulted when no manifest matches.
	 *
	 * @deprecated Scheduled for removal in Umbraco 19, along with its implementations. Register an
	 * `auditLogType` manifest per log type instead of overriding this method.
	 */
	getTagStyleAndText?(logType: string): UmbAuditLogTagData;
}
