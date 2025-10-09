import { extractUmbNotificationColor } from './extractUmbNotificationColor.function.js';
import { isUmbNotifications, UMB_NOTIFICATION_HEADER } from './isUmbNotifications.function.js';
import { isProblemDetailsLike } from './apiTypeValidators.function.js';
import type { UmbProblemDetails } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbNotificationColor } from '@umbraco-cms/backoffice/notification';
import type { umbHttpClient } from '@umbraco-cms/backoffice/http-client';

const MAX_RETRIES = 3;

export class UmbApiInterceptorController extends UmbControllerBase {
	/**
	 * Store pending requests that received a 401 response and are waiting for re-authentication.
	 * This is used to retry the requests after re-authentication.
	 */
	#pending401Requests: Array<{
		request: Request;
		requestConfig: unknown;
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
	#nonGet401Requests: Array<{ request: Request; requestConfig: unknown }> = [];

	/**
	 * Binds the default interceptors to the client.
	 * This includes the auth response interceptor, the error interceptor and the umb-notifications interceptor.
	 * @param {umbHttpClient} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 */
	public bindDefaultInterceptors(client: typeof umbHttpClient) {
		// Add the default observables to the instance
		this.handleUnauthorizedAuthRetry();
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

			// Build a plain ProblemDetails object for the response body
			const problemDetails: UmbProblemDetails = {
				status: response.status,
				title: response.statusText || 'Unauthorized request, waiting for re-authentication.',
				detail: undefined,
				errors: undefined,
				type: 'Unauthorized',
				stack: undefined,
			};

			const newResponse = this.#createResponse(problemDetails, response);

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
				this.#pending401Requests.push({
					request,
					requestConfig,
					retry: async () => {
						const { data, response: retryResponse } = await client.request(requestConfig as never);

						return this.#createResponse(data, retryResponse);
					},
					resolve,
					reject,
					retries,
				});

				// Show login overlay
				authContext.timeOut();

				console.log(
					'[Interceptor] 401 Unauthorized - queuing request for re-authentication and have tried',
					retries - 1,
					'times before',
					requestConfig,
				);
			});
		});
	}

	/**
	 * Interceptor which checks responses for 403 errors and displays them as a notification.
	 * @param {umbHttpClient} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 * @internal
	 */
	addForbiddenResponseInterceptor(client: typeof umbHttpClient) {
		client.interceptors.response.use((response): Response => {
			if (response.status !== 403) return response;

			// Build a plain ProblemDetails object for the response body
			const problemDetails: UmbProblemDetails = {
				status: response.status,
				title:
					response.statusText ||
					'You do not have the necessary permissions to complete the requested action. If you believe this is in error, please reach out to your administrator.',
				detail: undefined,
				errors: undefined,
				type: 'Unauthorized',
				stack: undefined,
			};

			return this.#createResponse(problemDetails, response);
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

			// Return a new response with the generated resource in the body (plain text)
			return this.#createResponse(generatedResource, response);
		});
	}

	/**
	 * Interceptor which checks responses for 500 errors and displays them as a notification if any.
	 * @param {umbHttpClient} client The OpenAPI client to add the interceptor to. It can be any client supporting Response and Request interceptors.
	 * @internal
	 */
	addErrorInterceptor(client: typeof umbHttpClient) {
		client.interceptors.response.use(async (response): Promise<Response> => {
			// If the response is ok, we just return the response
			if (response.ok) return response;

			// We will check if it is not a 401 or 403 error, as that is handled by other interceptors
			if (response.status === 401 || response.status === 403) return response;

			// Special handling for 404 Not Found
			if (response.status === 404) {
				const notFoundProblemDetails: UmbProblemDetails = {
					status: response.status,
					title: response.statusText || 'The requested resource was not found.',
					detail: undefined,
					errors: undefined,
					type: 'NotFound',
					stack: undefined,
				};
				return this.#createResponse(notFoundProblemDetails, response);
			}

			// For all other errors, we will build a ProblemDetails object
			let problemDetails: UmbProblemDetails = {
				status: response.status,
				title:
					response.statusText ||
					'A fatal server error occurred. If this continues, please reach out to your administrator.',
				detail: undefined,
				errors: undefined,
				type: 'ServerError',
				stack: undefined,
			};

			try {
				// Clones the response to read the body
				const origResponse = response.clone();
				const errorBody = await origResponse.json();

				// If there is JSON in the error, we will try to parse it as a ProblemDetails object
				if (errorBody && isProblemDetailsLike(errorBody)) {
					// Merge the parsed problem details into our default
					problemDetails = errorBody;
				}
			} catch (e) {
				// Ignore JSON parse error
				console.error('[Interceptor] Caught a server error, but failed parsing error body (expected JSON)', e);
			}

			return this.#createResponse(problemDetails, response);
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
	 * Listen for authorization signal to retry GET-requests that received a 401 Unauthorized response.
	 * This will retry all pending requests that received a 401 Unauthorized response after re-authentication.
	 * It will also notify the user about non-GET requests that received a 401 Unauthorized response.
	 * @internal
	 */
	handleUnauthorizedAuthRetry() {
		this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.observe(
				context?.authorizationSignal,
				() => {
					console.log('[Interceptor] 401 Unauthorized - re-authentication completed');

					// On auth, retry all pending requests
					const requests = this.#pending401Requests.splice(0, this.#pending401Requests.length);
					requests.forEach((req) => {
						console.log('[Interceptor] 401 Unauthorized - retrying request after re-authentication', req.requestConfig);
						req.retry().then(req.resolve).catch(req.reject);
					});

					// Notify about non-GET 401s after successful re-auth
					if (this.#nonGet401Requests.length > 0) {
						const errors: Record<string, string> = {};
						this.#nonGet401Requests.forEach((req) => {
							errors[`${req.request.method} ${req.request.url}`] = `Request failed with 401 Unauthorized.`;
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

	/**
	 * Helper to create a new Response with correct Content-Type.
	 * @param {unknown} body The body of the response, can be a string or an object.
	 * @param {Response} originalResponse The original response to copy status and headers from.
	 * @returns {Response} The new Response object with the correct Content-Type and body.
	 */
	#createResponse(body: unknown, originalResponse: Response): Response {
		const isString = typeof body === 'string';
		const contentType = isString ? 'text/plain' : 'application/json';
		const responseBody = isString ? body : JSON.stringify(body);

		// Construct new headers but preserve "X-" headers from the original response
		const headersOverride: Record<string, string> = {};
		originalResponse.headers.forEach((value, key) => {
			if (key.toLowerCase().startsWith('x-')) {
				headersOverride[key] = value;
			}
		});

		return new Response(responseBody, {
			status: originalResponse.status,
			statusText: originalResponse.statusText,
			headers: {
				...headersOverride,
				'Content-Type': contentType,
			},
		});
	}

	/**
	 * Helper to show a notification error.
	 * @param headline
	 * @param message
	 * @param details
	 * @param color
	 */
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
