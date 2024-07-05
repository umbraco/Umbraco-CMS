import type { UmbNotificationsEventModel } from './resource.controller.js';
import { EventMessageTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

export function isUmbNotification(notification: unknown): notification is UmbNotificationsEventModel {
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
