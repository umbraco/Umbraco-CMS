import type { UmbNotificationColor } from '../notification/types.js';
import { EventMessageTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * @deprecated Import from `@umbraco-cms/backoffice/resources` instead.
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
