import { UmbDocumentAuditLog, type UmbDocumentAuditLogType } from '../../../audit-log/utils/index.js';

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

// Return label, color, look, desc

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

		case UmbDocumentAuditLog.PUBLISH:
			return {
				style: { look: 'primary', color: 'positive' },
				text: { label: 'content_publish', desc: 'auditTrails_publish' },
			};

		case UmbDocumentAuditLog.UNPUBLISH:
			return {
				style: { look: 'primary', color: 'warning' },
				text: { label: 'content_unpublish', desc: 'auditTrails_unpublish' },
			};

		case UmbDocumentAuditLog.CONTENT_VERSION_ENABLE_CLEANUP:
			return {
				style: { look: 'secondary', color: 'default' },
				text: {
					label: 'contentTypeEditor_historyCleanupEnableCleanup',
					desc: 'auditTrails_contentversionenablecleanup',
				},
			};

		case UmbDocumentAuditLog.CONTENT_VERSION_PREVENT_CLEANUP:
			return {
				style: { look: 'secondary', color: 'default' },
				text: {
					label: 'contentTypeEditor_historyCleanupPreventCleanup',
					desc: 'auditTrails_contentversionpreventcleanup',
				},
			};

		default:
			return {
				style: { look: 'placeholder', color: 'danger' },
				text: { label: type, desc: 'TODO' },
			};
	}
}

export const TimeOptions: Intl.DateTimeFormatOptions = {
	year: 'numeric',
	month: 'long',
	day: 'numeric',
	hour: 'numeric',
	minute: 'numeric',
	second: 'numeric',
};
