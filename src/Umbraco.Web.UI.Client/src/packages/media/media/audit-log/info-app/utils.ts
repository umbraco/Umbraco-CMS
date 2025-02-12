import type { UmbMediaAuditLogType } from '../utils/index.js';
import { UmbMediaAuditLog } from '../utils/index.js';

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
 */
export function getMediaHistoryTagStyleAndText(type: UmbMediaAuditLogType): HistoryData {
	switch (type) {
		case UmbMediaAuditLog.SAVE:
			return {
				style: { look: 'primary', color: 'default' },
				text: { label: 'auditTrails_smallSave', desc: 'auditTrails_save' },
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
