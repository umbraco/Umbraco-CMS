import type { UmbNotificationColor } from '../notification/types.js';
import { EventMessageTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Extracts the UmbNotificationColor from the EventMessageTypeModel.
 * @param {EventMessageTypeModel} type The EventMessageTypeModel to extract the color from.
 * @returns {UmbNotificationColor} The corresponding UmbNotificationColor.
 * @example
 * const color = extractUmbNotificationColor(EventMessageTypeModel.ERROR); // color will be 'danger'
 */
export function extractUmbNotificationColor(type: EventMessageTypeModel): UmbNotificationColor {
	switch (type) {
		case EventMessageTypeModel.ERROR:
			return 'danger';
		case EventMessageTypeModel.WARNING:
			return 'warning';
		case EventMessageTypeModel.INFO:
		case EventMessageTypeModel.DEFAULT:
			return 'default';
		case EventMessageTypeModel.SUCCESS:
			return 'positive';
		default:
			return '';
	}
}
