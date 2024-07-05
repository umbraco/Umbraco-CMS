import { isUmbNotification } from './isUmbNotification.function.js';
import type { UmbNotificationsEventModel } from './resource.controller.js';

export function isUmbNotifications(notifications: Array<unknown>): notifications is Array<UmbNotificationsEventModel> {
	return notifications.every(isUmbNotification);
}
