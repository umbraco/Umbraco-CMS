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

		case UmbMediaAuditLog.COPY:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallCopy', desc: 'auditTrails_copy' },
			};

		case UmbMediaAuditLog.MOVE:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallMove', desc: 'auditTrails_move' },
			};

		case UmbMediaAuditLog.DELETE:
			return {
				style: { look: 'secondary', color: 'danger' },
				text: { label: 'auditTrails_smallDelete', desc: 'auditTrails_delete' },
			};

		case UmbMediaAuditLog.SORT:
			return {
				style: { look: 'secondary', color: 'default' },
				text: { label: 'auditTrails_smallSort', desc: 'auditTrails_sort' },
			};

		case UmbMediaAuditLog.CUSTOM:
			return {
				style: { look: 'placeholder', color: 'default' },
				text: { label: 'auditTrails_smallCustom', desc: 'auditTrails_custom' },
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
