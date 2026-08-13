import type { UmbMediaAuditLogModel } from '../types.js';
import type { UmbMediaAuditLogType } from '../utils/index.js';
import { getMediaHistoryTagStyleAndText } from '../info-app/utils.js';
import { UmbMediaAuditLogServerDataSource } from './media-audit-log.server.data-source.js';
import type {
	UmbAuditLogRepository,
	UmbAuditLogRequestArgs,
	UmbAuditLogTagData,
} from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase, type UmbRepositoryResponse, type UmbPagedModel } from '@umbraco-cms/backoffice/repository';

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
	 * @param {UmbAuditLogRequestArgs} args - The audit log request arguments
	 * @returns {UmbRepositoryResponse<UmbPagedModel<UmbMediaAuditLogModel>>} The audit log for the media
	 * @memberof UmbMediaAuditLogRepository
	 */
	async requestAuditLog(
		args: UmbAuditLogRequestArgs,
	): Promise<UmbRepositoryResponse<UmbPagedModel<UmbMediaAuditLogModel>>> {
		return this.#dataSource.getAuditLog(args);
	}

	/**
	 * Get the tag style and localization data for a given audit log type
	 * @param {string} logType - The audit log type
	 * @returns {UmbAuditLogTagData} The tag style and localization data
	 * @memberof UmbMediaAuditLogRepository
	 */
	getTagStyleAndText(logType: string): UmbAuditLogTagData {
		return getMediaHistoryTagStyleAndText(logType as UmbMediaAuditLogType);
	}
}

export { UmbMediaAuditLogRepository as api };
