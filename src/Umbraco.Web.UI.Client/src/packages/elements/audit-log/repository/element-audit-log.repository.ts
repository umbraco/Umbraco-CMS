import type { UmbElementAuditLogModel } from '../types.js';
import { UmbElementAuditLog } from '../utils/index.js';
import { UmbElementAuditLogServerDataSource } from './element-audit-log.server.data-source.js';
import type {
	UmbAuditLogRepository,
	UmbAuditLogRequestArgs,
	UmbAuditLogTagData,
} from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

const UMB_ELEMENT_AUDITLOG_TAG_LOOKUP: Readonly<Record<string, UmbAuditLogTagData>> = Object.freeze({
	[UmbElementAuditLog.SAVE]: {
		style: { look: 'primary', color: 'default' },
		text: { label: 'auditTrails_smallSave', desc: 'auditTrailsElement_save' },
	},
	[UmbElementAuditLog.SAVE_VARIANT]: {
		style: { look: 'primary', color: 'default' },
		text: { label: 'auditTrails_smallSaveVariant', desc: 'auditTrailsElement_savevariant' },
	},
	[UmbElementAuditLog.PUBLISH]: {
		style: { look: 'primary', color: 'positive' },
		text: { label: 'auditTrails_smallPublish', desc: 'auditTrailsElement_publish' },
	},
	[UmbElementAuditLog.UNPUBLISH]: {
		style: { look: 'primary', color: 'warning' },
		text: { label: 'auditTrails_smallUnpublish', desc: 'auditTrailsElement_unpublish' },
	},
	[UmbElementAuditLog.PUBLISH_VARIANT]: {
		style: { look: 'primary', color: 'positive' },
		text: { label: 'auditTrails_smallPublishVariant', desc: 'auditTrailsElement_publishvariant' },
	},
	[UmbElementAuditLog.UNPUBLISH_VARIANT]: {
		style: { look: 'primary', color: 'warning' },
		text: { label: 'auditTrails_smallUnpublishVariant', desc: 'auditTrailsElement_unpublishvariant' },
	},
	[UmbElementAuditLog.CONTENT_VERSION_ENABLE_CLEANUP]: {
		style: { look: 'secondary', color: 'default' },
		text: {
			label: 'auditTrails_smallContentVersionEnableCleanup',
			desc: 'auditTrailsElement_contentversionenablecleanup',
		},
	},
	[UmbElementAuditLog.CONTENT_VERSION_PREVENT_CLEANUP]: {
		style: { look: 'secondary', color: 'default' },
		text: {
			label: 'auditTrails_smallContentVersionPreventCleanup',
			desc: 'auditTrailsElement_contentversionpreventcleanup',
		},
	},
	[UmbElementAuditLog.COPY]: {
		style: { look: 'secondary', color: 'default' },
		text: { label: 'auditTrails_smallCopy', desc: 'auditTrailsElement_copy' },
	},
	[UmbElementAuditLog.MOVE]: {
		style: { look: 'secondary', color: 'default' },
		text: { label: 'auditTrails_smallMove', desc: 'auditTrailsElement_move' },
	},
	[UmbElementAuditLog.DELETE]: {
		style: { look: 'secondary', color: 'danger' },
		text: { label: 'auditTrails_smallDelete', desc: 'auditTrailsElement_delete' },
	},
	[UmbElementAuditLog.ROLL_BACK]: {
		style: { look: 'secondary', color: 'default' },
		text: { label: 'auditTrails_smallRollBack', desc: 'auditTrailsElement_rollback' },
	},
	[UmbElementAuditLog.CUSTOM]: {
		style: { look: 'placeholder', color: 'default' },
		text: { label: 'auditTrails_smallCustom', desc: 'auditTrailsElement_custom' },
	},
});

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
	 * Request the audit log for an element
	 * @param {UmbAuditLogRequestArgs} args
	 * @returns {*}
	 * @memberof UmbElementAuditLogRepository
	 */
	async requestAuditLog(args: UmbAuditLogRequestArgs) {
		return this.#dataSource.getAuditLog(args);
	}

	/**
	 * Get the tag style and localization data for a given audit log type
	 * @param {string} logType
	 * @returns {UmbAuditLogTagData}
	 * @memberof UmbElementAuditLogRepository
	 */
	getTagStyleAndText(logType: string): UmbAuditLogTagData {
		return (
			UMB_ELEMENT_AUDITLOG_TAG_LOOKUP[logType] ?? {
				style: { look: 'placeholder', color: 'default' },
				text: { label: logType, desc: '' },
			}
		);
	}
}

export { UmbElementAuditLogRepository as api };
