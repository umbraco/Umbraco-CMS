import type { UmbAuditLogRequestArgs } from '../types.js';
import { UmbMediaAuditLogServerDataSource } from './media-audit-log.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * Repository for the Media audit log
 * @export
 * @class UmbMediaAuditLogRepository
 * @extends {UmbRepositoryBase}
 */
export class UmbMediaAuditLogRepository extends UmbRepositoryBase {
	#dataSource: UmbMediaAuditLogServerDataSource;

	/**
	 * Creates an instance of UmbMediaAuditLogRepository.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaAuditLogRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#dataSource = new UmbMediaAuditLogServerDataSource(host);
	}

	/**
	 * Request the audit log for a Media
	 * @param {UmbAuditLogRequestArgs} args
	 * @return {*}
	 * @memberof UmbMediaAuditLogRepository
	 */
	async requestAuditLog(args: UmbAuditLogRequestArgs) {
		return this.#dataSource.getAuditLog(args);
	}
}
