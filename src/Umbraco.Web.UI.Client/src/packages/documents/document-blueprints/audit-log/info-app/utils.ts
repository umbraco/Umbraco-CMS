import { UmbDocumentBlueprintAuditLog, type UmbDocumentBlueprintAuditLogType } from '../utils/index.js';

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
export function getDocumentBlueprintHistoryTagStyleAndText(type: UmbDocumentBlueprintAuditLogType): HistoryData {
	switch (type) {
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
				text: { label: type, desc: '' },
			};
	}
}
