import { UmbElementAuditLog, type UmbElementAuditLogType } from '../utils/index.js';

interface HistoryStyleMap {
	look: 'default' | 'primary' | 'secondary' | 'outline' | 'placeholder';
	color: 'default' | 'danger' | 'warning' | 'positive';
}

interface HistoryLocalizeKeys {
	label: string;
	desc: string;
}

interface HistoryData {
	style: HistoryStyleMap;
	text: HistoryLocalizeKeys;
}

/**
 * @description Helper function to get look and color for uui-tag and localization keys for the label and description.
 * @param type AuditTypeModel
 * @returns {HistoricData}
 */
export function getElementHistoryTagStyleAndText(type: UmbElementAuditLogType): HistoryData {
	switch (type) {
		case UmbElementAuditLog.SAVE:
			return {
				style: { look: 'primary', color: 'default' },
				text: { label: 'auditTrails_smallSave', desc: 'auditTrails_save' },
			};

		case UmbElementAuditLog.SAVE_VARIANT:
			return {
				style: { look: 'primary', color: 'default' },
				text: { label: 'auditTrails_smallSaveVariant', desc: 'auditTrails_savevariant' },
			};

		case UmbElementAuditLog.PUBLISH:
			return {
				style: { look: 'primary', color: 'positive' },
				text: { label: 'auditTrails_smallPublish', desc: 'auditTrails_publish' },
			};

		case UmbElementAuditLog.UNPUBLISH:
			return {
				style: { look: 'primary', color: 'warning' },
				text: { label: 'auditTrails_smallUnpublish', desc: 'auditTrails_unpublish' },
			};

		case UmbElementAuditLog.PUBLISH_VARIANT:
			return {
				style: { look: 'primary', color: 'positive' },
				text: { label: 'auditTrails_smallPublishVariant', desc: 'auditTrails_publishvariant' },
			};

		case UmbElementAuditLog.UNPUBLISH_VARIANT:
			return {
				style: { look: 'primary', color: 'warning' },
				text: { label: 'auditTrails_smallUnpublishVariant', desc: 'auditTrails_unpublishvariant' },
			};

		case UmbElementAuditLog.CONTENT_VERSION_ENABLE_CLEANUP:
			return {
				style: { look: 'secondary', color: 'default' },
				text: {
					label: 'auditTrails_smallContentVersionEnableCleanup',
					desc: 'auditTrails_contentversionenablecleanup',
				},
			};

		case UmbElementAuditLog.CONTENT_VERSION_PREVENT_CLEANUP:
			return {
				style: { look: 'secondary', color: 'default' },
				text: {
					label: 'auditTrails_smallContentVersionPreventCleanup',
					desc: 'auditTrails_contentversionpreventcleanup',
				},
			};

		case UmbElementAuditLog.COPY:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallCopy', desc: 'auditTrails_copy' },
			};

		case UmbElementAuditLog.MOVE:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallMove', desc: 'auditTrails_move' },
			};

		case UmbElementAuditLog.DELETE:
			return {
				style: { look: 'secondary', color: 'danger' },
				text: { label: 'auditTrails_smallDelete', desc: 'auditTrails_delete' },
			};

		case UmbElementAuditLog.ROLL_BACK:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallRollBack', desc: 'auditTrails_rollback' },
			};

		case UmbElementAuditLog.SORT:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallSort', desc: 'auditTrails_sort' },
			};

		case UmbElementAuditLog.CUSTOM:
			return {
				style: { look: 'placeholder', color: 'default' },
				text: { label: 'auditTrails_smallCustom', desc: 'auditTrails_custom' },
			};

		default:
			return {
				style: { look: 'placeholder', color: 'default' },
				text: { label: type, desc: '' },
			};
	}
}
