import { UmbApiInterceptorController } from './api-interceptor.controller.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { umbHttpClient } from '@umbraco-cms/backoffice/http-client';

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
});
