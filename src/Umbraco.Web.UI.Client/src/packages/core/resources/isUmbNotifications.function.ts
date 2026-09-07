import { EventMessageTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Checks if a value is a valid UmbNotificationsEventModel object.
 * @param {unknown} notification - The value to check.
 * @returns {boolean} True if the value is a valid UmbNotificationsEventModel object.
 */
function objectIsUmbNotification(notification: unknown): notification is UmbNotificationsEventModel {
	if (typeof notification !== 'object' || notification === null) {
		return false;
	}
	const object = notification as UmbNotificationsEventModel;
	return (
		typeof object.category === 'string' &&
		typeof object.message === 'string' &&
		typeof object.type === 'string' &&
		Object.values(EventMessageTypeModel).includes(object.type)
	);
}

export interface UmbNotificationsEventModel {
	category: string;
	message: string;
	type: EventMessageTypeModel;
}

/**
 * Checks if an array of unknown values are all valid UmbNotificationsEventModel objects.
 * @param {Array<unknown>} notifications The array to check.
 * @returns {boolean} True if all items are valid UmbNotificationsEventModel objects.
 */
export function isUmbNotifications(notifications: Array<unknown>): notifications is Array<UmbNotificationsEventModel> {
	return notifications.every(objectIsUmbNotification);
}

export const UMB_NOTIFICATION_HEADER = 'umb-notifications';
