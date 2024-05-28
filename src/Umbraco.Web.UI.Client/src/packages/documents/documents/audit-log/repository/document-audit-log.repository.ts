import type { UmbDocumentAuditLogModel } from '../types.js';
import { UmbDocumentAuditLogServerDataSource } from './document-audit-log.server.data-source.js';
import type { UmbAuditLogRepository, UmbAuditLogRequestArgs } from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * Repository for the document audit log
 * @export
 * @class UmbDocumentAuditLogRepository
 * @extends {UmbRepositoryBase}
 */
export class UmbDocumentAuditLogRepository
	extends UmbRepositoryBase
	implements UmbAuditLogRepository<UmbDocumentAuditLogModel>
{
	#dataSource: UmbDocumentAuditLogServerDataSource;

	/**
	 * Creates an instance of UmbDocumentAuditLogRepository.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentAuditLogRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#dataSource = new UmbDocumentAuditLogServerDataSource(host);
	}

	/**
	 * Request the audit log for a document
	 * @param {UmbAuditLogRequestArgs} args
	 * @return {*}
	 * @memberof UmbDocumentAuditLogRepository
	 */
	async requestAuditLog(args: UmbAuditLogRequestArgs) {
		return this.#dataSource.getAuditLog(args);
	}
}
