import { EventMessageTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 *
 * @param notification
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
 *
 * @param notifications
 */
export function isUmbNotifications(notifications: Array<unknown>): notifications is Array<UmbNotificationsEventModel> {
	return notifications.every(objectIsUmbNotification);
}

export const UMB_NOTIFICATION_HEADER = 'umb-notifications';
