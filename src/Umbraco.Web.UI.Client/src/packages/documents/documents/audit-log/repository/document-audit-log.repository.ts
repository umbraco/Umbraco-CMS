import type { UmbDocumentAuditLogModel } from '../types.js';
import { UmbDocumentAuditLog, type UmbDocumentAuditLogType } from '../utils/index.js';
import { UmbDocumentAuditLogServerDataSource } from './document-audit-log.server.data-source.js';
import type {
	UmbAuditLogRepository,
	UmbAuditLogRequestArgs,
	UmbAuditLogTagData,
} from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Repository for the document audit log
 * @class UmbDocumentAuditLogRepository
 * @augments {UmbRepositoryBase}
 */
export class UmbDocumentAuditLogRepository
	extends UmbRepositoryBase
	implements UmbAuditLogRepository<UmbDocumentAuditLogModel>
{
	#dataSource: UmbDocumentAuditLogServerDataSource;

	/**
	 * Creates an instance of UmbDocumentAuditLogRepository.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentAuditLogRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#dataSource = new UmbDocumentAuditLogServerDataSource(host);
	}

	/**
	 * Request the audit log for a document
	 * @param {UmbAuditLogRequestArgs} args - The request arguments
	 * @returns {Promise<UmbRepositoryResponse<UmbPagedModel<UmbDocumentAuditLogModel>>>} The audit log data
	 * @memberof UmbDocumentAuditLogRepository
	 */
	async requestAuditLog(
		args: UmbAuditLogRequestArgs,
	): Promise<UmbRepositoryResponse<UmbPagedModel<UmbDocumentAuditLogModel>>> {
		return this.#dataSource.getAuditLog(args);
	}

	/**
	 * Get the tag style and localization data for a given audit log type
	 * @param {string} logType - The audit log type
	 * @returns {UmbAuditLogTagData} The tag style and localization data
	 * @memberof UmbDocumentAuditLogRepository
	 */
	getTagStyleAndText(logType: string): UmbAuditLogTagData {
		// TODO: Investigate why the repository is responsible for presentation (tag style) logic, and consider moving it out of the repository contract.
		switch (logType as UmbDocumentAuditLogType) {
			case UmbDocumentAuditLog.SAVE:
				return {
					style: { look: 'primary', color: 'default' },
					text: { label: 'auditTrails_smallSave', desc: 'auditTrails_save' },
				};

			case UmbDocumentAuditLog.SAVE_VARIANT:
				return {
					style: { look: 'primary', color: 'default' },
					text: { label: 'auditTrails_smallSaveVariant', desc: 'auditTrails_savevariant' },
				};

			case UmbDocumentAuditLog.PUBLISH:
				return {
					style: { look: 'primary', color: 'positive' },
					text: { label: 'auditTrails_smallPublish', desc: 'auditTrails_publish' },
				};

			case UmbDocumentAuditLog.UNPUBLISH:
				return {
					style: { look: 'primary', color: 'warning' },
					text: { label: 'auditTrails_smallUnpublish', desc: 'auditTrails_unpublish' },
				};

			case UmbDocumentAuditLog.PUBLISH_VARIANT:
				return {
					style: { look: 'primary', color: 'positive' },
					text: { label: 'auditTrails_smallPublishVariant', desc: 'auditTrails_publishvariant' },
				};

			case UmbDocumentAuditLog.UNPUBLISH_VARIANT:
				return {
					style: { look: 'primary', color: 'warning' },
					text: { label: 'auditTrails_smallUnpublishVariant', desc: 'auditTrails_unpublishvariant' },
				};

			case UmbDocumentAuditLog.CONTENT_VERSION_ENABLE_CLEANUP:
				return {
					style: { look: 'secondary', color: 'default' },
					text: {
						label: 'auditTrails_smallContentVersionEnableCleanup',
						desc: 'auditTrails_contentversionenablecleanup',
					},
				};

			case UmbDocumentAuditLog.CONTENT_VERSION_PREVENT_CLEANUP:
				return {
					style: { look: 'secondary', color: 'default' },
					text: {
						label: 'auditTrails_smallContentVersionPreventCleanup',
						desc: 'auditTrails_contentversionpreventcleanup',
					},
				};

			case UmbDocumentAuditLog.ASSIGN_DOMAIN:
				return {
					style: { look: 'secondary', color: 'default' },
					text: { label: 'auditTrails_smallAssignDomain', desc: 'auditTrails_assigndomain' },
				};

			case UmbDocumentAuditLog.COPY:
				return {
					style: { look: 'secondary', color: 'default' },
					text: { label: 'auditTrails_smallCopy', desc: 'auditTrails_copy' },
				};

			case UmbDocumentAuditLog.MOVE:
				return {
					style: { look: 'secondary', color: 'default' },
					text: { label: 'auditTrails_smallMove', desc: 'auditTrails_move' },
				};

			case UmbDocumentAuditLog.DELETE:
				return {
					style: { look: 'secondary', color: 'danger' },
					text: { label: 'auditTrails_smallDelete', desc: 'auditTrails_delete' },
				};

			case UmbDocumentAuditLog.ROLL_BACK:
				return {
					style: { look: 'secondary', color: 'default' },
					text: { label: 'auditTrails_smallRollBack', desc: 'auditTrails_rollback' },
				};

			case UmbDocumentAuditLog.SEND_TO_PUBLISH:
				return {
					style: { look: 'secondary', color: 'positive' },
					text: { label: 'auditTrails_smallSendToPublish', desc: 'auditTrails_sendtopublish' },
				};

			case UmbDocumentAuditLog.SEND_TO_PUBLISH_VARIANT:
				return {
					style: { look: 'secondary', color: 'positive' },
					text: { label: 'auditTrails_smallSendToPublishVariant', desc: 'auditTrails_sendtopublishvariant' },
				};

			case UmbDocumentAuditLog.SORT:
				return {
					style: { look: 'secondary', color: 'default' },
					text: { label: 'auditTrails_smallSort', desc: 'auditTrails_sort' },
				};

			case UmbDocumentAuditLog.CUSTOM:
				return {
					style: { look: 'placeholder', color: 'default' },
					text: { label: 'auditTrails_smallCustom', desc: 'auditTrails_custom' },
				};

			default:
				return {
					style: { look: 'placeholder', color: 'default' },
					text: { label: logType, desc: '' },
				};
		}
	}
}

export { UmbDocumentAuditLogRepository as api };
