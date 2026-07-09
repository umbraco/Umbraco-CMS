import { UmbDocumentBlueprintAuditLogServerDataSource } from './document-blueprint-audit-log.server.data-source.js';
import type { UmbDocumentBlueprintAuditLogModel } from './types.js';
import type {
	UmbAuditLogRepository,
	UmbAuditLogRequestArgs,
	UmbAuditLogTagData,
} from '@umbraco-cms/backoffice/audit-log';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const UmbDocumentBlueprintAuditLog = Object.freeze({
	CUSTOM: 'Custom',
	MOVE: 'Move',
	NEW: 'New',
	OPEN: 'Open',
	SAVE_VARIANT: 'SaveVariant',
	SAVE: 'Save',
	SYSTEM: 'System',
});

/**
 * Repository for the document blueprint audit log.
 * @class UmbDocumentBlueprintAuditLogRepository
 * @augments {UmbRepositoryBase}
 */
export class UmbDocumentBlueprintAuditLogRepository
	extends UmbRepositoryBase
	implements UmbAuditLogRepository<UmbDocumentBlueprintAuditLogModel>
{
	#dataSource: UmbDocumentBlueprintAuditLogServerDataSource;

	/**
	 * Creates an instance of UmbDocumentBlueprintAuditLogRepository.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentBlueprintAuditLogRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#dataSource = new UmbDocumentBlueprintAuditLogServerDataSource(host);
	}

	/**
	 * Request the audit log for a document blueprint.
	 * @param {UmbAuditLogRequestArgs} args
	 * @returns {*}
	 * @memberof UmbDocumentBlueprintAuditLogRepository
	 */
	async requestAuditLog(args: UmbAuditLogRequestArgs) {
		return this.#dataSource.getAuditLog(args);
	}

	/**
	 * Get the tag style and localization data for a given audit log type.
	 * @param {string} logType
	 * @returns {UmbAuditLogTagData}
	 * @memberof UmbDocumentBlueprintAuditLogRepository
	 */
	getTagStyleAndText(logType: string): UmbAuditLogTagData {
		switch (logType) {
			case UmbDocumentBlueprintAuditLog.SAVE:
				return {
					style: { look: 'primary', color: 'default' },
					text: { label: 'auditTrails_smallSave', desc: 'auditTrails_save' },
				};

			case UmbDocumentBlueprintAuditLog.SAVE_VARIANT:
				return {
					style: { look: 'primary', color: 'default' },
					text: { label: 'auditTrails_smallSaveVariant', desc: 'auditTrails_savevariant' },
				};

			case UmbDocumentBlueprintAuditLog.MOVE:
				return {
					style: { look: 'secondary', color: 'default' },
					text: { label: 'auditTrails_smallMove', desc: 'auditTrails_move' },
				};

			case UmbDocumentBlueprintAuditLog.CUSTOM:
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

export { UmbDocumentBlueprintAuditLogRepository as api };
