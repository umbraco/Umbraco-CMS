import type { UmbNotificationColor } from '../notification/notification.context.js';
import { EventMessageTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

export function extractUmbColorNotification(type: EventMessageTypeModel): UmbNotificationColor {
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
