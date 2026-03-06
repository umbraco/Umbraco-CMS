import type { UmbMediaAuditLogModel } from '../types.js';
import type { UmbMediaAuditLogType } from '../utils/index.js';
import { getMediaHistoryTagStyleAndText } from '../info-app/utils.js';
import { UmbMediaAuditLogServerDataSource } from './media-audit-log.server.data-source.js';
import type { UmbAuditLogRepository, UmbAuditLogRequestArgs, UmbAuditLogTagData } from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * Repository for the Media audit log
 * @class UmbMediaAuditLogRepository
 * @augments {UmbRepositoryBase}
 */
export class UmbMediaAuditLogRepository
	extends UmbRepositoryBase
	implements UmbAuditLogRepository<UmbMediaAuditLogModel>
{
	#dataSource: UmbMediaAuditLogServerDataSource;

	/**
	 * Creates an instance of UmbMediaAuditLogRepository.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaAuditLogRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#dataSource = new UmbMediaAuditLogServerDataSource(host);
	}

	/**
	 * Request the audit log for a Media
	 * @param {UmbAuditLogRequestArgs} args
	 * @returns {*}
	 * @memberof UmbMediaAuditLogRepository
	 */
	async requestAuditLog(args: UmbAuditLogRequestArgs) {
		return this.#dataSource.getAuditLog(args);
	}

	/**
	 * Get the tag style and localization data for a given audit log type
	 * @param {string} logType
	 * @returns {UmbAuditLogTagData}
	 * @memberof UmbMediaAuditLogRepository
	 */
	getTagStyleAndText(logType: string): UmbAuditLogTagData {
		return getMediaHistoryTagStyleAndText(logType as UmbMediaAuditLogType);
	}
}

export { UmbMediaAuditLogRepository as api };
