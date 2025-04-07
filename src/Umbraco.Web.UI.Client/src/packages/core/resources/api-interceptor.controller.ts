import { extractUmbNotificationColor } from './extractUmbNotificationColor.function.js';
import { isUmbNotifications, UMB_NOTIFICATION_HEADER } from './isUmbNotifications.function.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { OpenAPIConfig } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbNotificationColor } from '@umbraco-cms/backoffice/notification';

export class UmbApiInterceptorController extends UmbControllerBase {
	public bindDefaultInterceptors(client: OpenAPIConfig) {
		this.addAuthResponseInterceptor(client);
		this.addUmbNotificationsInterceptor(client);
		this.addErrorInterceptor(client);
	}

	/**
	 * @internal
	 */
	addAuthResponseInterceptor(client: OpenAPIConfig) {
		client.interceptors.response.use(async (response) => {
			if (response.status === 401) {
				// See if we can get the UmbAuthContext and let it know the user is timed out
				const authContext = await this.getContext(UMB_AUTH_CONTEXT, { preventTimeout: true });
				if (!authContext) {
					throw new Error('Could not get the auth context');
				}
				authContext.timeOut();
			}
			return response;
		});
	}

	/**
	 * @internal
	 */
	addErrorInterceptor(client: OpenAPIConfig) {
		client.interceptors.response.use(async (response) => {
			if (response.ok) return response;

			// Clones the response to read the body
			const origResponse = response.clone();
			const error = await origResponse.json();
			if (!error) return response;

			// If the error is not an UmbError, we check if it is a problem details object
			// Check if the error is a problem details object
			if (!('type' in error) || !('title' in error) || !('status' in error)) {
				// If not, we just return the response
				return response;
			}

			// Handle 500 errors - we need to show a notification
			if (origResponse.status === 500) {
				let headline = error.title ?? error.name ?? 'Server Error';
				let message = 'A fatal server error occurred. If this continues, please reach out to your administrator.';

				// Special handling for ObjectCacheAppCache corruption errors, which we are investigating
				if (
					error.detail?.includes('ObjectCacheAppCache') ||
					error.detail?.includes('Umbraco.Cms.Infrastructure.Scoping.Scope.DisposeLastScope()')
				) {
					headline = 'Please restart the server';
					message =
						'The Umbraco object cache is corrupt, but your action may still have been executed. Please restart the server to reset the cache. This is a work in progress.';
				}

				this.#peekError(headline, message, error.errors ?? error.detail);
			}

			// Return original response
			return response;
		});
	}

	/**
	 * Interceptor which checks responses for the umb-notifications header and displays them as a notification if any. Removes the umb-notifications from the headers.
	 * @internal
	 */
	addUmbNotificationsInterceptor(client: OpenAPIConfig) {
		client.interceptors.response.use((response) => {
			const umbNotifications = response.headers.get(UMB_NOTIFICATION_HEADER);
			if (!umbNotifications) return response;

			const notifications = JSON.parse(umbNotifications);
			if (!isUmbNotifications(notifications)) return response;

			for (const notification of notifications) {
				this.#peekError(
					notification.category,
					notification.message,
					null,
					extractUmbNotificationColor(notification.type),
				);
			}

			const newHeader = new Headers();
			for (const header of response.headers.entries()) {
				const [key, value] = header;
				if (key !== UMB_NOTIFICATION_HEADER) newHeader.set(key, value);
			}

			const newResponse = response.clone();
			newResponse.headers.delete(UMB_NOTIFICATION_HEADER);
			return newResponse;

			/*const newResponse = new Response(response.body, {
				headers: newHeader,
				status: response.status,
				statusText: response.statusText,
			});

			return newResponse;*/
		});
	}

	async #peekError(headline: string, message: string, details: unknown, color?: UmbNotificationColor) {
		// This late importing is done to avoid circular reference [NL]
		(await import('@umbraco-cms/backoffice/notification')).umbPeekError(this, {
			headline,
			message,
			details,
			color,
		});
	}
}
