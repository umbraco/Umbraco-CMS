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
				text: { label: 'auditTrails_smallSave', desc: 'auditTrailsElement_save' },
			};

		case UmbElementAuditLog.SAVE_VARIANT:
			return {
				style: { look: 'primary', color: 'default' },
				text: { label: 'auditTrails_smallSaveVariant', desc: 'auditTrailsElement_savevariant' },
			};

		case UmbElementAuditLog.PUBLISH:
			return {
				style: { look: 'primary', color: 'positive' },
				text: { label: 'auditTrails_smallPublish', desc: 'auditTrailsElement_publish' },
			};

		case UmbElementAuditLog.UNPUBLISH:
			return {
				style: { look: 'primary', color: 'warning' },
				text: { label: 'auditTrails_smallUnpublish', desc: 'auditTrailsElement_unpublish' },
			};

		case UmbElementAuditLog.PUBLISH_VARIANT:
			return {
				style: { look: 'primary', color: 'positive' },
				text: { label: 'auditTrails_smallPublishVariant', desc: 'auditTrailsElement_publishvariant' },
			};

		case UmbElementAuditLog.UNPUBLISH_VARIANT:
			return {
				style: { look: 'primary', color: 'warning' },
				text: { label: 'auditTrails_smallUnpublishVariant', desc: 'auditTrailsElement_unpublishvariant' },
			};

		case UmbElementAuditLog.CONTENT_VERSION_ENABLE_CLEANUP:
			return {
				style: { look: 'secondary', color: 'default' },
				text: {
					label: 'auditTrails_smallContentVersionEnableCleanup',
					desc: 'auditTrailsElement_contentversionenablecleanup',
				},
			};

		case UmbElementAuditLog.CONTENT_VERSION_PREVENT_CLEANUP:
			return {
				style: { look: 'secondary', color: 'default' },
				text: {
					label: 'auditTrails_smallContentVersionPreventCleanup',
					desc: 'auditTrailsElement_contentversionpreventcleanup',
				},
			};

		case UmbElementAuditLog.COPY:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallCopy', desc: 'auditTrailsElement_copy' },
			};

		case UmbElementAuditLog.MOVE:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallMove', desc: 'auditTrailsElement_move' },
			};

		case UmbElementAuditLog.DELETE:
			return {
				style: { look: 'secondary', color: 'danger' },
				text: { label: 'auditTrails_smallDelete', desc: 'auditTrailsElement_delete' },
			};

		case UmbElementAuditLog.ROLL_BACK:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallRollBack', desc: 'auditTrailsElement_rollback' },
			};

		case UmbElementAuditLog.CUSTOM:
			return {
				style: { look: 'placeholder', color: 'default' },
				text: { label: 'auditTrails_smallCustom', desc: 'auditTrailsElement_custom' },
			};

		default:
			return {
				style: { look: 'placeholder', color: 'default' },
				text: { label: type, desc: '' },
			};
	}
}
