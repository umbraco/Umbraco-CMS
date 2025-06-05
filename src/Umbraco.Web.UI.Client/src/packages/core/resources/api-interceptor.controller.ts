import { extractUmbNotificationColor } from './extractUmbNotificationColor.function.js';
import { isUmbNotifications, UMB_NOTIFICATION_HEADER } from './isUmbNotifications.function.js';
import { isProblemDetailsLike } from './apiTypeValidators.function.js';
import { UmbApiError } from './umb-error.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbNotificationColor } from '@umbraco-cms/backoffice/notification';
import type { RequestOptions, umbHttpClient } from '@umbraco-cms/backoffice/http-client';

const MAX_RETRIES = 3;
const AUTH_WAIT_TIMEOUT = 120000; // 120 seconds

export class UmbApiInterceptorController extends UmbControllerBase {
	/**
	 * Store pending requests that received a 401 response and are waiting for re-authentication.
	 * This is used to retry the requests after re-authentication.
	 */
	#pending401Requests: Array<{
		request: Request;
		requestConfig: RequestOptions;
		retry: () => Promise<Response>;
		resolve: (value: Response) => void;
		reject: (reason?: unknown) => void;
		retries: number;
	}> = [];

	/**
	 * Store non-GET requests that received a 401 response.
	 * This is used to notify the user about actions that could not be completed due to session expiration.
	 * These requests will not be retried, as they are not idempotent.
	 * Instead, we will notify the user about these requests after re-authentication.
	 */
	#nonGet401Requests: Array<{ request: Request; requestConfig: RequestOptions }> = [];

	/**
	 * Binds the default interceptors to the client.
	 * This includes the auth response interceptor, the error interceptor and the umb-notifications interceptor.
	 * @param {umbHttpClient} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 */
	public bindDefaultInterceptors(client: typeof umbHttpClient) {
		// Add default observables to the instance
		this.observeNonGet401Requests();
		// Add the default interceptors to the client
		this.addAuthResponseInterceptor(client);
		this.addForbiddenResponseInterceptor(client);
		this.addUmbGeneratedResourceInterceptor(client);
		this.addUmbNotificationsInterceptor(client);
		this.addErrorInterceptor(client);
	}

	/**
	 * Interceptor which checks responses for 401 errors and lets the UmbAuthContext know the user is timed out.
	 * @param {umbHttpClient} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 * @internal
	 */
	addAuthResponseInterceptor(client: typeof umbHttpClient) {
		client.interceptors.response.use(async (response, request, requestConfig): Promise<Response> => {
			if (response.status !== 401) return response;

			const error = new UmbApiError(response.statusText, response.status, request, {
				status: response.status,
				title: response.statusText,
				detail: 'Unauthorized request, waiting for re-authentication.',
				errors: undefined,
				type: 'Unauthorized',
				stack: undefined,
			});

			const newResponse = new Response(JSON.stringify(error), {
				...response,
				headers: {
					...Object.fromEntries(response.headers.entries()),
					'Content-Type': 'application/json',
				},
			});

			// See if we can get the UmbAuthContext and let it know the user is timed out
			const authContext = await this.getContext(UMB_AUTH_CONTEXT, { preventTimeout: true });
			if (!authContext) throw new Error('Could not get the auth context');

			// Only retry for GET requests
			if (request.method !== 'GET') {
				// Collect info for later notification
				this.#nonGet401Requests.push({ request, requestConfig });

				// Show login overlay (only once per burst, as before)
				authContext.timeOut();
				return newResponse;
			}

			// Find if this request is already in the queue and increment retries
			let retries = 1;
			const existing = this.#pending401Requests.find(
				(req) => req.request === request && req.requestConfig === requestConfig,
			);
			if (existing) {
				retries = existing.retries + 1;
				if (retries > MAX_RETRIES) {
					return newResponse;
				}
				existing.retries = retries;
			}

			// Return a promise that will resolve when re-auth completes
			return new Promise<Response>((resolve, reject) => {
				// Before pushing to the queue
				const wasQueueEmpty = this.#pending401Requests.length === 0;

				this.#pending401Requests.push({
					request,
					requestConfig,
					retry: async () => {
						const { data, response } = await client.request(requestConfig as never);

						// Manually create a new response object with the data because the original response has been read
						let body: string;
						if (typeof data === 'string') {
							body = data;
						} else {
							body = JSON.stringify(data);
						}

						if (response) {
							return new Response(body, {
								...response,
								headers: {
									...Object.fromEntries(response.headers.entries()),
									'Content-Type': 'application/json',
								},
							});
						}

						throw new Error('Response is not available for retry');
					},
					resolve,
					reject,
					retries,
				});

				// If the queue was empty, we need to signal the auth context that we are timing out
				// This is to ensure that the auth context is only signaled once, even if multiple requests are queued
				// and the auth context is not already timing out
				if (wasQueueEmpty) {
					// Show login overlay
					authContext.timeOut();
				}

				console.log(
					'[Interceptor] 401 Unauthorized - queuing request for re-authentication and have tried',
					retries - 1,
					'times before',
					requestConfig,
				);

				// Wait for auth signal or timeout
				Promise.race([
					firstValueFrom(authContext.authorizationSignal),
					new Promise((_, rej) => setTimeout(() => rej(error), AUTH_WAIT_TIMEOUT)),
				])
					.then(() => {
						console.log('[Interceptor] 401 Unauthorized - re-authentication completed');

						// On auth, retry all pending requests
						const requests = this.#pending401Requests.splice(0, this.#pending401Requests.length);
						requests.forEach((req) => {
							console.log(
								'[Interceptor] 401 Unauthorized - retrying request after re-authentication',
								req.requestConfig,
							);
							req.retry().then(req.resolve).catch(req.reject);
						});
					})
					.catch((err) => {
						console.error('[Interceptor] 401 Unauthorized - re-authentication failed', err);
						// On timeout, reject all pending requests
						const requests = this.#pending401Requests.splice(0, this.#pending401Requests.length);
						requests.forEach((req) => req.reject(err));
						this.#nonGet401Requests.length = 0; // Clear on failure too
					});
			});
		});
	}

	/**
	 * Interceptor which checks responses for 403 errors and displays them as a notification.
	 * @param {umbHttpClient} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 * @internal
	 */
	addForbiddenResponseInterceptor(client: typeof umbHttpClient) {
		client.interceptors.response.use((response, request): Response => {
			if (response.status !== 403) return response;

			const error = new UmbApiError(response.statusText, response.status, request, {
				status: response.status,
				title: response.statusText,
				detail:
					'You do not have the necessary permissions to complete the requested action. If you believe this is in error, please reach out to your administrator.',
				errors: undefined,
				type: 'Unauthorized',
				stack: undefined,
			});

			const newResponse = new Response(JSON.stringify(error), {
				...response,
				headers: {
					...Object.fromEntries(response.headers.entries()),
					'Content-Type': 'application/json',
				},
			});

			return newResponse;
		});
	}

	/**
	 * Interceptor which checks responses for the Umb-Generated-Resource header and replaces the value into the response body.
	 * @param {umbHttpClient} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 * @internal
	 */
	addUmbGeneratedResourceInterceptor(client: typeof umbHttpClient) {
		client.interceptors.response.use((response): Response => {
			if (!response.headers.has('Umb-Generated-Resource')) return response;

			const generatedResource = response.headers.get('Umb-Generated-Resource');
			if (generatedResource === null) {
				return response;
			}

			// Generate new response body with the generated resource, which is a guid
			const newResponse = new Response(generatedResource, {
				...response,
				headers: {
					...Object.fromEntries(response.headers.entries()),
					'Content-Type': 'plain/text',
				},
			});

			return newResponse;
		});
	}

	/**
	 * Interceptor which checks responses for 500 errors and displays them as a notification if any.
	 * @param {umbHttpClient} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 * @internal
	 */
	addErrorInterceptor(client: typeof umbHttpClient) {
		client.interceptors.response.use(async (response, request): Promise<Response> => {
			// If the response is ok, we just return the response
			if (response.ok) return response;

			// We will check if it is not a 401 error, as that is handled by the auth interceptor
			if (response.status === 401) return response;

			const apiError = new UmbApiError(response.statusText, response.status, request, {
				status: response.status,
				title: response.statusText,
				detail: 'A fatal server error occurred. If this continues, please reach out to your administrator.',
				errors: undefined,
				type: 'ServerError',
				stack: undefined,
			});

			try {
				// Clones the response to read the body
				const origResponse = response.clone();
				const errorBody = await origResponse.json();

				// If there is JSON in the error, we will try to parse it as a ProblemDetails object
				if (errorBody && isProblemDetailsLike(errorBody)) {
					// Set the problem details on the apiError
					apiError.problemDetails = errorBody;
				}
			} catch (e) {
				// Ignore JSON parse error
				console.error('[Interceptor] Caught a 500 Error, but failed parsing error body (expected JSON)', e);
			}

			// Return the error response
			return new Response(JSON.stringify(apiError), {
				...response,
				headers: {
					...Object.fromEntries(response.headers.entries()),
					'Content-Type': 'application/json',
				},
			});
		});
	}

	/**
	 * Interceptor which checks responses for the umb-notifications header and displays them as a notification if any. Removes the umb-notifications from the headers.
	 * @param {umbHttpClient} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 * @internal
	 */
	addUmbNotificationsInterceptor(client: typeof umbHttpClient) {
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

	/**
	 * Listen for authorization signal to clear non-GET 401 requests
	 */
	observeNonGet401Requests() {
		this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.observe(
				context?.authorizationSignal,
				() => {
					// Notify about non-GET 401s after successful re-auth
					if (this.#nonGet401Requests.length > 0) {
						const errors: Record<string, string> = {};
						this.#nonGet401Requests.forEach((req) => {
							errors[`${req.requestConfig.method} ${req.requestConfig.url}`] = `Request failed with 401 Unauthorized.`;
						});
						this.#peekError(
							'Some actions were not completed',
							'Some actions could not be completed because your session expired. Please try again.',
							errors,
							'warning',
						);
						this.#nonGet401Requests.length = 0; // Clear after notifying
					}
				},
				'_authClearNonGet401Requests',
			);
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
