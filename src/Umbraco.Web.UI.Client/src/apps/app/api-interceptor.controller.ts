import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { OpenAPI } from '@umbraco-cms/backoffice/external/backend-api';
import {
	extractUmbNotificationColor,
	isUmbNotifications,
	UMB_NOTIFICATION_CONTEXT,
	UMB_NOTIFICATION_HEADER,
} from '@umbraco-cms/backoffice/notification';

/**
 * Controller that adds interceptors to the OpenAPI client
 */
export class UmbApiInterceptorController extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);
		this.#addUmbNotificationsInterceptor();
	}

	/**
	 * Interceptor which checks responses for the umb-notifications header and displays them as a notification if any. Removes the umb-notifications from the headers.
	 */
	#addUmbNotificationsInterceptor() {
		OpenAPI.interceptors.response.use((response) => {
			const umbNotifications = response.headers.get(UMB_NOTIFICATION_HEADER);
			if (!umbNotifications) return response;

			const notifications = JSON.parse(umbNotifications);
			if (!isUmbNotifications(notifications)) return response;

			this.getContext(UMB_NOTIFICATION_CONTEXT).then((notificationContext) => {
				for (const notification of notifications) {
					notificationContext.peek(extractUmbNotificationColor(notification.type), {
						data: { headline: notification.category, message: notification.message },
					});
				}
			});

			const newHeader = new Headers();
			for (const header of response.headers.entries()) {
				const [key, value] = header;
				if (key !== UMB_NOTIFICATION_HEADER) newHeader.set(key, value);
			}

			const newResponse = new Response(response.body, {
				headers: newHeader,
				status: response.status,
				statusText: response.statusText,
			});

			return newResponse;
		});
	}
}
