import { UmbApiInterceptorController } from './api-interceptor.controller.js';
import { UMB_AUTH_SIGNALER_CONTEXT, type UmbAuthSignalerContext } from './auth-signaler.context.js';
import { createClient, type Client } from '../backend-api/client/index.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { Subscription } from '@umbraco-cms/backoffice/external/rxjs';
import type { umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { UMB_NOTIFICATION_CONTEXT, type UmbPeekErrorArgs } from '@umbraco-cms/backoffice/notification';

type ResponseInterceptor = (response: Response, request: Request, options: unknown) => Response | Promise<Response>;

@customElement('test-api-interceptor-host')
class UmbTestApiInterceptorHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbApiInterceptorController', () => {
	let hostElement: UmbTestApiInterceptorHostElement;
	let controller: UmbApiInterceptorController;
	let responseInterceptors: Array<ResponseInterceptor>;
	let fakeClient: typeof umbHttpClient;

	beforeEach(() => {
		hostElement = new UmbTestApiInterceptorHostElement();
		document.body.appendChild(hostElement);
		controller = new UmbApiInterceptorController(hostElement);

		responseInterceptors = [];
		fakeClient = {
			interceptors: {
				response: {
					use: (fn: ResponseInterceptor) => responseInterceptors.push(fn),
				},
			},
		} as unknown as typeof umbHttpClient;

		controller.addErrorInterceptor(fakeClient);

		expect(responseInterceptors).to.have.lengthOf(1);
	});

	afterEach(() => {
		hostElement.remove();
	});

	it('rewrites a Cloudflare gateway-timeout (524) response into a friendly ProblemDetails body', async () => {
		const originalResponse = new Response('<html>524: A timeout occurred</html>', {
			status: 524,
			headers: { 'Content-Type': 'text/html' },
		});

		const result = await responseInterceptors[0](originalResponse, new Request('https://example.com'), {});
		const body = await result.json();

		expect(result.status).to.equal(524);
		expect(body.type).to.equal('GatewayTimeout');
		expect(body.title).to.not.include('<html>');
		// The status code is preserved in the detail so it can be reported/searched on, e.g. in a support ticket.
		expect(body.detail).to.include('524');
	});

	it('rewrites a gateway-timeout (504) response the same way', async () => {
		const originalResponse = new Response('<html>504 Gateway Time-out</html>', {
			status: 504,
			headers: { 'Content-Type': 'text/html' },
		});

		const result = await responseInterceptors[0](originalResponse, new Request('https://example.com'), {});
		const body = await result.json();

		expect(body.type).to.equal('GatewayTimeout');
	});

	it('rewrites a gateway-unreachable (523) response with a message that does not claim the action ran', async () => {
		const originalResponse = new Response('<html>523: Origin is unreachable</html>', {
			status: 523,
			headers: { 'Content-Type': 'text/html' },
		});

		const result = await responseInterceptors[0](originalResponse, new Request('https://example.com'), {});
		const body = await result.json();

		expect(body.type).to.equal('GatewayUnreachable');
		expect(body.detail).to.include('523');
		expect(body.detail).to.not.include('may still have completed');
	});

	it('does not special-case a 408 Request Timeout, since that is sent by the origin itself, not a gateway', async () => {
		const originalResponse = new Response(JSON.stringify({ type: 'ServerError', title: 'Request Timeout', status: 408 }), {
			status: 408,
			headers: { 'Content-Type': 'application/json' },
		});

		const result = await responseInterceptors[0](originalResponse, new Request('https://example.com'), {});
		const body = await result.json();

		expect(body.type).to.not.equal('GatewayTimeout');
		expect(body.type).to.not.equal('GatewayUnreachable');
	});

	it('leaves other error responses to fall through to the generic ServerError branch', async () => {
		const originalResponse = new Response(JSON.stringify({ type: 'ServerError', title: 'Boom', status: 500 }), {
			status: 500,
			headers: { 'Content-Type': 'application/json' },
		});

		const result = await responseInterceptors[0](originalResponse, new Request('https://example.com'), {});
		const body = await result.json();

		expect(body.type).to.equal('ServerError');
		expect(body.title).to.equal('Boom');
	});

	it('leaves ok responses untouched', async () => {
		const originalResponse = new Response('{}', { status: 200 });

		const result = await responseInterceptors[0](originalResponse, new Request('https://example.com'), {});

		expect(result).to.equal(originalResponse);
	});

	// An opaque redirect and a network error both report status 0, which `new Response()` rejects.
	it('rewrites a response without an HTTP status into a server error instead of throwing', async () => {
		const result = await responseInterceptors[0](Response.error(), new Request('https://example.com'), {});

		expect(result.status).to.equal(500);
		expect((await result.json()).type).to.equal('ServerError');
	});

	describe('authentication', () => {
		const LOGIN_URL = 'https://example.com/umbraco/login?ReturnUrl=%2Fumbraco%2Fmanagement%2Fapi%2Fv1%2Fdocument';

		let signaler: UmbAuthSignalerContext;
		let timeoutRequests: number;
		let activityDetectedAt: number | undefined;
		let subscriptions: Array<Subscription>;

		beforeEach(async () => {
			signaler = (await new UmbContextConsumerController(hostElement, UMB_AUTH_SIGNALER_CONTEXT).asPromise())!;
			timeoutRequests = 0;
			activityDetectedAt = undefined;
			subscriptions = [
				signaler.timeoutRequest.subscribe(() => timeoutRequests++),
				signaler.activityDetectedAt.subscribe((at) => (activityDetectedAt = at)),
			];
		});

		afterEach(() => {
			subscriptions.forEach((subscription) => subscription.unsubscribe());
		});

		/**
		 * The client follows redirects, and the login page answers 200, so an auth challenge only shows up
		 * as where the request ended up. Only fetch can produce a followed redirect, so fake the two
		 * properties it sets on one.
		 * @param {string} url The URL the request ended up at.
		 * @returns {Response} A 200 response that reports having been redirected to `url`.
		 */
		function redirectedResponse(url: string): Response {
			const response = new Response('<!doctype html><title>Log in</title>', {
				status: 200,
				headers: { 'Content-Type': 'text/html' },
			});
			Object.defineProperties(response, {
				redirected: { value: true },
				url: { value: url },
			});
			return response;
		}

		describe('redirect to the login page', () => {
			const requestConfig = { url: '/umbraco/management/api/v1/document' };

			let chain: Array<ResponseInterceptor>;
			let retriedConfigs: Array<unknown>;

			beforeEach(() => {
				chain = [];
				retriedConfigs = [];
				const chainClient = {
					interceptors: {
						response: {
							use: (fn: ResponseInterceptor) => chain.push(fn),
						},
					},
					request: async (config: unknown) => {
						retriedConfigs.push(config);
						return {
							data: { retried: true },
							response: new Response('{"retried":true}', {
								status: 200,
								headers: { 'Content-Type': 'application/json' },
							}),
						};
					},
				} as unknown as typeof umbHttpClient;

				controller.bindDefaultInterceptors(chainClient);
			});

			async function runChain(response: Response, request: Request): Promise<Response> {
				for (const interceptor of chain) {
					response = await interceptor(response, request, requestConfig);
				}
				return response;
			}

			it('treats a GET that ended up on the login page as unauthorized and retries it after re-authentication', async () => {
				const request = new Request('https://example.com/umbraco/management/api/v1/document/123');

				const result = runChain(redirectedResponse(LOGIN_URL), request);
				await aTimeout(0);

				expect(timeoutRequests).to.equal(1);
				expect(activityDetectedAt).to.be.undefined;
				expect(retriedConfigs).to.be.empty;

				signaler.setAuthorized(true);
				const response = await result;

				expect(retriedConfigs).to.have.lengthOf(1);
				expect(retriedConfigs[0]).to.include(requestConfig);
				expect(response.status).to.equal(200);
				expect(await response.json()).to.deep.equal({ retried: true });
			});

			it('answers a non-GET that ended up on the login page with a 401 and reports it after re-authentication', async () => {
				const peeks: Array<UmbPeekErrorArgs> = [];
				new UmbContextProviderController(hostElement, UMB_NOTIFICATION_CONTEXT, {
					getHostElement: () => hostElement,
					peek: (_color: unknown, options: { data: UmbPeekErrorArgs }) => peeks.push(options.data),
				} as never);
				const request = new Request('https://example.com/umbraco/management/api/v1/document', { method: 'POST' });

				const response = await runChain(redirectedResponse(LOGIN_URL), request);

				expect(response.status).to.equal(401);
				const body = await response.json();
				expect(body.status).to.equal(401);
				expect(body.type).to.equal('Unauthorized');
				expect(timeoutRequests).to.equal(1);
				expect(activityDetectedAt).to.be.undefined;

				signaler.setAuthorized(true);
				await waitUntil(() => peeks.length > 0, 'the failed action was never reported');

				expect(retriedConfigs).to.be.empty;
				expect(peeks[0].errors).to.have.property(`POST ${request.url}`);
			});

			it('recognises the login page under a path base and with a trailing slash', async () => {
				const request = new Request('https://example.com/cms/umbraco/management/api/v1/document', {
					method: 'POST',
				});

				const response = await runChain(
					redirectedResponse('https://example.com/cms/umbraco/login/?ReturnUrl=%2F'),
					request,
				);

				expect(response.status).to.equal(401);
				expect(timeoutRequests).to.equal(1);
			});

			it('passes a response redirected anywhere other than the login page through as a success', async () => {
				const redirected = redirectedResponse('https://example.com/media/exports/report.csv?from=%2Fumbraco%2Flogin');

				const response = await runChain(
					redirected,
					new Request('https://example.com/umbraco/management/api/v1/my-extension/export'),
				);

				expect(response).to.equal(redirected);
				expect(activityDetectedAt).to.be.a('number');
				expect(timeoutRequests).to.equal(0);
			});
		});

		// A retry goes through the client, and so through the interceptors, again. Whatever it gets then is
		// final: asking for re-authentication a second time would only repeat the same round trip.
		describe('retry after re-authentication', () => {
			const DOCUMENT_URL = '/umbraco/management/api/v1/document/123';

			let client: Client;
			let fetched: Array<string>;
			let responses: Array<Response>;

			beforeEach(() => {
				fetched = [];
				responses = [];
				// The generated client itself, so a retry makes a real second round trip. Configured like
				// umbHttpClient, which throws the problem details of a failed request.
				client = createClient({
					baseUrl: 'https://example.com',
					throwOnError: true,
					fetch: (async (request: Request) => {
						fetched.push(request.url);
						return responses.shift() ?? new Response(null, { status: 500 });
					}) as typeof fetch,
				});

				controller.bindDefaultInterceptors(client as unknown as typeof umbHttpClient);
			});

			/**
			 * Makes a GET, lets the user re-authenticate once it has been queued for that, and waits for the
			 * retry to have been answered.
			 * @returns {Promise<{ settled: Promise<unknown> }>} What the call settles with: its result, or what
			 * it threw. Wrapped, so a call that never settles can still be inspected.
			 */
			async function getThroughReauthentication(): Promise<{ settled: Promise<unknown> }> {
				const settled = client.get({ url: DOCUMENT_URL }).then(
					(value) => value,
					(error: unknown) => error,
				);
				await waitUntil(() => timeoutRequests > 0, 'the request was never queued for re-authentication');

				signaler.setAuthorized(true);
				await waitUntil(() => fetched.length === 2, 'the request was never retried');
				await aTimeout(0);

				return { settled };
			}

			it('resolves with the retried response when re-authentication fixed the request', async () => {
				responses.push(
					redirectedResponse(LOGIN_URL),
					new Response('{"name":"Home"}', { status: 200, headers: { 'Content-Type': 'application/json' } }),
				);

				const { settled } = await getThroughReauthentication();

				expect(timeoutRequests).to.equal(1);
				expect(((await settled) as { data: unknown }).data).to.deep.equal({ name: 'Home' });
			});

			// The retried request carries the session that was just re-established, so ending up on the
			// login page again means the user is signed in but not allowed (the server's access-denied
			// path is the login page too).
			it('gives up with a 403 when a GET still ends up on the login page after re-authentication', async () => {
				responses.push(redirectedResponse(LOGIN_URL), redirectedResponse(LOGIN_URL));

				const { settled } = await getThroughReauthentication();

				expect(timeoutRequests).to.equal(1);
				const error = (await settled) as { status: number; title: string };
				expect(error.status).to.equal(403);
				expect(error.title).to.include('permissions');
				expect(activityDetectedAt).to.be.undefined;
			});

			it('gives up with the 401 when a GET is still unauthorized after re-authentication', async () => {
				const stillUnauthorized = { status: 401, title: 'Still unauthorized', type: 'Unauthorized' };
				responses.push(
					new Response(null, { status: 401 }),
					new Response(JSON.stringify(stillUnauthorized), {
						status: 401,
						headers: { 'Content-Type': 'application/json' },
					}),
				);

				const { settled } = await getThroughReauthentication();

				expect(timeoutRequests).to.equal(1);
				expect(await settled).to.deep.equal(stillUnauthorized);
			});
		});
	});
});
