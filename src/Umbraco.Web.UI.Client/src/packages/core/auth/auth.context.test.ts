import { UmbAuthContext, type UmbAuthSession } from './auth.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-auth-context-host')
class UmbTestAuthContextHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbAuthContext', () => {
	let context: UmbAuthContext;
	let hostElement: UmbTestAuthContextHostElement;

	beforeEach(() => {
		hostElement = new UmbTestAuthContextHostElement();
		context = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', false);
	});

	afterEach(() => {
		context.destroy();
	});

	describe('Public API', () => {
		it('has an isAuthorized property', () => {
			expect(context).to.have.property('isAuthorized');
		});

		it('has an isInitialized property', () => {
			expect(context).to.have.property('isInitialized');
		});

		it('has a getIsAuthorized method', () => {
			expect(context).to.have.property('getIsAuthorized').that.is.a('function');
		});

		it('has a setInitialState method', () => {
			expect(context).to.have.property('setInitialState').that.is.a('function');
		});

		it('has a getLatestToken method', () => {
			expect(context).to.have.property('getLatestToken').that.is.a('function');
		});

		it('has a clearTokenStorage method', () => {
			expect(context).to.have.property('clearTokenStorage').that.is.a('function');
		});

		it('has a signOut method', () => {
			expect(context).to.have.property('signOut').that.is.a('function');
		});

		it('has a getServerUrl method', () => {
			expect(context).to.have.property('getServerUrl').that.is.a('function');
		});

		it('has a configureClient method', () => {
			expect(context).to.have.property('configureClient').that.is.a('function');
		});

		it('has a session$ observable', () => {
			expect(context).to.have.property('session$');
		});

		it('has an authorizationSignal property (deprecated)', () => {
			expect(context).to.have.property('authorizationSignal');
		});
	});

	describe('getServerUrl', () => {
		it('returns the configured server URL', () => {
			const serverUrl = context.getServerUrl();
			expect(serverUrl).to.equal('http://localhost');
		});
	});

	describe('getIsAuthorized', () => {
		it('returns a boolean value', () => {
			const isAuthorized = context.getIsAuthorized();
			expect(isAuthorized).to.be.a('boolean');
		});

		it('returns false when no session is set', () => {
			const isAuthorized = context.getIsAuthorized();
			expect(isAuthorized).to.be.false;
		});

		it('returns true when auth is bypassed', () => {
			const bypassedContext = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', true);
			const isAuthorized = bypassedContext.getIsAuthorized();
			expect(isAuthorized).to.be.true;
			bypassedContext.destroy();
		});
	});

	describe('getOpenApiConfiguration', () => {
		it('returns configuration with server URL', () => {
			const config = context.getOpenApiConfiguration();
			expect(config.base).to.equal('http://localhost');
			expect(config.credentials).to.equal('include');
			expect(config.token).to.be.a('function');
		});
	});

	describe('configureClient', () => {
		it('sets the config on a client', () => {
			let receivedConfig: Record<string, unknown> = {};
			const mockClient = {
				buildUrl: () => '',
				getConfig: () => ({}),
				request: () => Promise.resolve({}) as never,
				interceptors: { request: { use: () => {} }, response: { use: () => {} } },
				setConfig: (config: Record<string, unknown>) => {
					receivedConfig = config;
					return config;
				},
			};

			context.configureClient(mockClient as never);

			expect(receivedConfig).to.have.property('baseUrl', 'http://localhost');
			expect(receivedConfig).to.have.property('credentials', 'include');
			expect(receivedConfig).to.have.property('auth').that.is.a('function');
		});
	});

	describe('clearTokenStorage', () => {
		it('clears the session and broadcasts', () => {
			expect(() => context.clearTokenStorage()).to.not.throw();
			expect(context.getIsAuthorized()).to.be.false;
		});
	});

	describe('Lifecycle', () => {
		it('can be destroyed without errors', () => {
			expect(() => context.destroy()).to.not.throw();

			// After destruction, create another context to ensure no conflicts
			const context2 = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', false);
			expect(context2).to.exist;
			context2.destroy();
		});

		it('handles multiple instances without conflicts', () => {
			const hostElement2 = new UmbTestAuthContextHostElement();
			const context2 = new UmbAuthContext(hostElement2, 'http://localhost', '/umbraco', false);

			expect(context).to.exist;
			expect(context2).to.exist;

			context2.destroy();
		});
	});

	describe('URL generation', () => {
		it('generates correct redirect URL', () => {
			const url = context.getRedirectUrl();
			expect(url).to.contain('/umbraco');
		});

		it('generates correct post-logout redirect URL', () => {
			const url = context.getPostLogoutRedirectUrl();
			expect(url).to.contain('/umbraco/logout');
		});
	});

	// Production calls the bare global `fetch`, so stubbing `window.fetch` intercepts it. The probe's
	// unauthenticated case is simulated with a 401 — an opaque redirect (status 0) can't be built in JS.
	describe('Server communication', () => {
		let originalFetch: typeof window.fetch;
		let fetchCalls: Array<{ input: RequestInfo | URL; init?: RequestInit }>;

		beforeEach(() => {
			originalFetch = window.fetch;
			fetchCalls = [];
		});

		afterEach(() => {
			window.fetch = originalFetch;
		});

		function stubFetch(respond: (input: RequestInfo | URL) => Response) {
			window.fetch = async (input: RequestInfo | URL, init?: RequestInit) => {
				fetchCalls.push({ input, init });
				return respond(input);
			};
		}

		function getLatestSession(): UmbAuthSession | undefined {
			let latestSession: UmbAuthSession | undefined;
			context.session$.subscribe((session) => (latestSession = session)).unsubscribe();
			return latestSession;
		}

		describe('setInitialState (session boot probe)', () => {
			it('probes user configuration and establishes a session from a 200 response', async () => {
				const timeoutUtc = new Date(Date.now() + 20 * 60 * 1000).toISOString();
				stubFetch(() => new Response(JSON.stringify({ timeoutUtc }), { status: 200 }));

				await context.setInitialState();

				expect(fetchCalls).to.have.lengthOf(1);
				expect(String(fetchCalls[0].input)).to.equal(
					'http://localhost/umbraco/management/api/v1/user/current/configuration',
				);
				expect(fetchCalls[0].init?.method).to.equal('GET');
				expect(fetchCalls[0].init?.credentials).to.equal('include');
				expect(fetchCalls[0].init?.redirect).to.equal('manual');

				expect(context.getIsAuthorized()).to.be.true;
				const session = getLatestSession();
				expect(session).to.not.be.undefined;
				expect(session!.expiresAt).to.be.greaterThan(Math.floor(Date.now() / 1000));
			});

			it('stays unauthorized and clears the session on a non-ok response', async () => {
				stubFetch(() => new Response(null, { status: 401 }));

				await context.setInitialState();

				expect(fetchCalls).to.have.lengthOf(1);
				expect(context.getIsAuthorized()).to.be.false;
				expect(getLatestSession()).to.be.undefined;
			});
		});

		describe('keepAlive', () => {
			it('POSTs to the keep-alive endpoint and refreshes the session on success', async () => {
				const timeoutUtc = new Date(Date.now() + 20 * 60 * 1000).toISOString();
				stubFetch((input) =>
					String(input).includes('keep-alive')
						? new Response(null, { status: 200 })
						: new Response(JSON.stringify({ timeoutUtc }), { status: 200 }),
				);

				const renewed = await context.keepAlive();

				expect(renewed).to.be.true;
				expect(String(fetchCalls[0].input)).to.equal(
					'http://localhost/umbraco/management/api/v1/security/back-office/keep-alive',
				);
				expect(fetchCalls[0].init?.method).to.equal('POST');
				expect(fetchCalls[0].init?.credentials).to.equal('include');
				expect(fetchCalls[0].init?.redirect).to.equal('manual');
				expect(context.getIsAuthorized()).to.be.true;
			});

			it('returns false when the keep-alive request fails', async () => {
				stubFetch(() => new Response(null, { status: 401 }));

				const renewed = await context.keepAlive();

				expect(renewed).to.be.false;
				// A failed keep-alive must not probe the configuration endpoint afterwards
				expect(fetchCalls).to.have.lengthOf(1);
				expect(context.getIsAuthorized()).to.be.false;
			});
		});
	});

	describe('Login URL construction', () => {
		let originalOpen: typeof window.open;
		let openedWindows: Array<{ url: string; target?: string; features?: string }>;
		// The deep-link decision reads window.location, so these tests navigate via history and
		// restore the runner's own path afterwards.
		const originalPath = window.location.pathname + window.location.search;

		beforeEach(() => {
			originalOpen = window.open;
			openedWindows = [];
			window.open = (url?: string | URL, target?: string, features?: string) => {
				openedWindows.push({ url: String(url), target, features });
				return null;
			};
		});

		afterEach(() => {
			window.open = originalOpen;
			history.replaceState(null, '', originalPath);
		});

		it('makeAuthorizationRequest (external, popup) opens the external-login challenge for the provider', async () => {
			await context.makeAuthorizationRequest('Google', false);

			expect(openedWindows).to.have.lengthOf(1);
			const url = new URL(openedWindows[0].url);
			expect(url.origin + url.pathname).to.equal(
				'http://localhost/umbraco/management/api/v1/security/back-office/external-login',
			);
			expect(url.searchParams.get('provider')).to.equal('Google');
			expect(openedWindows[0].target).to.equal('umbracoAuthPopup');
		});

		// Only routes behind the auth guard are worth returning to. `upgrade` matters as much as the
		// section routes: when the backend requires an upgrade the client routes there, so login has to
		// come back. The rest render without a session, so returning would show the login screen again.
		const returnUrlCases: Array<[path: string, expected: string | null]> = [
			['/umbraco/section/content/workspace/document/edit/123', '/umbraco/section/content/workspace/document/edit/123'],
			['/umbraco/upgrade', '/umbraco/upgrade'],
			['/umbraco/preview', '/umbraco/preview'],
			['/umbraco/logout', null],
			['/umbraco/error', null],
			['/umbraco/auth-callback', null],
			['/umbraco', null],
		];

		returnUrlCases.forEach(([path, expected]) => {
			it(`${expected ? 'carries' : 'omits'} returnUrl on ${path}`, async () => {
				history.replaceState(null, '', path);

				await context.makeAuthorizationRequest('Google', false);

				const url = new URL(openedWindows[0].url);
				expect(url.searchParams.get('returnUrl')).to.equal(expected);
			});
		});

		it('makeAuthorizationRequest (local, popup) opens the server login app with the auth-callback lander as ReturnUrl', async () => {
			await context.makeAuthorizationRequest('Umbraco', false);

			expect(openedWindows).to.have.lengthOf(1);
			const url = new URL(openedWindows[0].url);
			expect(url.origin + url.pathname).to.equal('http://localhost/umbraco/login');
			expect(url.searchParams.get('ReturnUrl')).to.equal(new URL('auth-callback', document.baseURI).pathname);
			expect(openedWindows[0].target).to.equal('umbracoAuthPopup');
		});
	});
});
