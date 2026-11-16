import type { UmbMediaAuditLogModel } from '../types.js';
import { UmbMediaAuditLog, type UmbMediaAuditLogType } from '../utils/index.js';
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
		// TODO: Investigate why the repository is responsible for presentation (tag style) logic, and consider moving it out of the repository contract.
		switch (logType as UmbMediaAuditLogType) {
			case UmbMediaAuditLog.SAVE:
				return {
					style: { look: 'primary', color: 'default' },
					text: { label: 'auditTrails_smallSave', desc: 'auditTrails_save' },
				};

			case UmbMediaAuditLog.COPY:
				return {
					style: { look: 'secondary', color: 'default' },
					text: { label: 'auditTrails_smallCopy', desc: 'auditTrails_copy' },
				};

			case UmbMediaAuditLog.MOVE:
				return {
					style: { look: 'secondary', color: 'default' },
					text: { label: 'auditTrails_smallMove', desc: 'auditTrails_move' },
				};

			case UmbMediaAuditLog.DELETE:
				return {
					style: { look: 'secondary', color: 'danger' },
					text: { label: 'auditTrails_smallDelete', desc: 'auditTrails_delete' },
				};

			case UmbMediaAuditLog.SORT:
				return {
					style: { look: 'secondary', color: 'default' },
					text: { label: 'auditTrails_smallSort', desc: 'auditTrails_sort' },
				};

			case UmbMediaAuditLog.CUSTOM:
				return {
					style: { look: 'placeholder', color: 'default' },
					text: { label: 'auditTrails_smallCustom', desc: 'auditTrails_custom' },
				};

			default:
				return {
					style: { look: 'placeholder', color: 'danger' },
					text: { label: logType, desc: 'TODO' },
				};
		}
	}
}

export { UmbMediaAuditLogRepository as api };
