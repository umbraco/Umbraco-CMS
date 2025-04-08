import { extractUmbNotificationColor } from './extractUmbNotificationColor.function.js';
import { isUmbNotifications, UMB_NOTIFICATION_HEADER } from './isUmbNotifications.function.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { OpenAPIConfig } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbNotificationColor } from '@umbraco-cms/backoffice/notification';

export class UmbApiInterceptorController extends UmbControllerBase {
	/**
	 * Binds the default interceptors to the client.
	 * This includes the auth response interceptor, the error interceptor and the umb-notifications interceptor.
	 * @param {OpenAPIConfig} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 */
	public bindDefaultInterceptors(client: OpenAPIConfig) {
		this.addAuthResponseInterceptor(client);
		this.addUmbNotificationsInterceptor(client);
		this.addErrorInterceptor(client);
	}

	/**
	 * Interceptor which checks responses for 401 errors and lets the UmbAuthContext know the user is timed out.
	 * @param {OpenAPIConfig} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
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
	 * Interceptor which checks responses for 500 errors and displays them as a notification if any.
	 * @param {OpenAPIConfig} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 * @internal
	 */
	addErrorInterceptor(client: OpenAPIConfig) {
		client.interceptors.response.use(async (response) => {
			if (response.ok) return response;

			// Handle 500 errors - we need to show a notification
			if (response.status === 500) {
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
	 * @param {OpenAPIConfig} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 * @internal
	 */
	addUmbNotificationsInterceptor(client: OpenAPIConfig) {
		client.interceptors.response.use((response) => {
			// Check if the response has the umb-notifications header
			// If not, we just return the response
			const umbNotifications = response.headers.get(UMB_NOTIFICATION_HEADER);
			if (!umbNotifications) return response;

			// Parse the notifications from the header
			// If the header is not a valid JSON, we just return the response
			try {
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
			} catch {
				// Ignore JSON parse errors
			}

			return response;
		});
	}

	async #peekError(headline: string, message: string, details: unknown, color?: UmbNotificationColor) {
		// Store the host for usage in the following async context
		const host = this._host;

		// This late importing is done to avoid circular reference [NL]
		(await import('@umbraco-cms/backoffice/notification')).umbPeekError(host, {
			headline,
			message,
			details,
			color,
		});
	}
}
