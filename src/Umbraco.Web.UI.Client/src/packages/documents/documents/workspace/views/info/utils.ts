import { AuditTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

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
export function HistoryTagStyleAndText(type: AuditTypeModel): HistoryData {
	switch (type) {
		case AuditTypeModel.SAVE:
			return {
				style: { look: 'primary', color: 'default' },
				text: { label: 'auditTrails_smallSave', desc: 'auditTrails_save' },
			};

		case AuditTypeModel.PUBLISH:
			return {
				style: { look: 'primary', color: 'positive' },
				text: { label: 'content_publish', desc: 'auditTrails_publish' },
			};

		case AuditTypeModel.UNPUBLISH:
			return {
				style: { look: 'primary', color: 'warning' },
				text: { label: 'content_unpublish', desc: 'auditTrails_unpublish' },
			};

		case AuditTypeModel.CONTENT_VERSION_ENABLE_CLEANUP:
			return {
				style: { look: 'secondary', color: 'default' },
				text: {
					label: 'contentTypeEditor_historyCleanupEnableCleanup',
					desc: 'auditTrails_contentversionenablecleanup',
				},
			};

		case AuditTypeModel.CONTENT_VERSION_PREVENT_CLEANUP:
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
