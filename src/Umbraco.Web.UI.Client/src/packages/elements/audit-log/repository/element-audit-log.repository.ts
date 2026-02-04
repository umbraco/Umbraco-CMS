import type { UmbElementAuditLogModel } from '../types.js';
import { UmbElementAuditLogServerDataSource } from './element-audit-log.server.data-source.js';
import type { UmbAuditLogRepository, UmbAuditLogRequestArgs } from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * Repository for the element audit log
 * @class UmbElementAuditLogRepository
 * @augments {UmbRepositoryBase}
 */
export class UmbElementAuditLogRepository
	extends UmbRepositoryBase
	implements UmbAuditLogRepository<UmbElementAuditLogModel>
{
	#dataSource: UmbElementAuditLogServerDataSource;

	/**
	 * Creates an instance of UmbElementAuditLogRepository.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementAuditLogRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#dataSource = new UmbElementAuditLogServerDataSource(host);
	}

	/**
	 * Request the audit log for a element
	 * @param {UmbAuditLogRequestArgs} args
	 * @returns {*}
	 * @memberof UmbElementAuditLogRepository
	 */
	async requestAuditLog(args: UmbAuditLogRequestArgs) {
		return this.#dataSource.getAuditLog(args);
	}
}
