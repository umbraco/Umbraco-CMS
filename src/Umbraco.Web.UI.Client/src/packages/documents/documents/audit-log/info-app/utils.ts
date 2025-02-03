import { UmbDocumentAuditLog, type UmbDocumentAuditLogType } from '../utils/index.js';

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
export function getDocumentHistoryTagStyleAndText(type: UmbDocumentAuditLogType): HistoryData {
	switch (type) {
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
				text: { label: type, desc: '' },
			};
	}
}
