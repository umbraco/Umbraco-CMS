import type { UmbNotificationColor } from './types.js';
import { EventMessageTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 *
 * @param type
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
