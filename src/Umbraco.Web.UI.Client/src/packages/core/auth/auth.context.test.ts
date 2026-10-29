import { UmbAuthContext, type UmbAuthSession } from './auth.context.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
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

		it('has a getIsAuthorized method', () => {
			expect(context).to.have.property('getIsAuthorized').that.is.a('function');
		});

		it('has a setInitialState method', () => {
			expect(context).to.have.property('setInitialState').that.is.a('function');
		});

		it('has a signOut method', () => {
			expect(context).to.have.property('signOut').that.is.a('function');
		});

		it('has a configureClient method', () => {
			expect(context).to.have.property('configureClient').that.is.a('function');
		});

		it('has a session$ observable', () => {
			expect(context).to.have.property('session$');
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
				interceptors: { request: { use: () => {} }, response: { use: () => {} }, error: { use: () => {} } },
				setConfig: (config: Record<string, unknown>) => {
					receivedConfig = config;
					return config;
				},
			};

			context.configureClient(mockClient as never);

			expect(receivedConfig).to.have.property('baseUrl', 'http://localhost');
			expect(receivedConfig).to.have.property('credentials', 'include');
			// Cookie auth carries no bearer token, so the auth callback is deliberately not set.
			expect(receivedConfig.auth).to.be.undefined;
			// Redirects are followed, so endpoints can answer with a 3xx; the auth interceptor recognises a
			// redirect to the login page instead.
			expect(receivedConfig.redirect ?? 'follow').to.equal('follow');
		});
	});

	// The server renews the session for any request that carries it, so the countdown has to follow
	// ordinary back office activity rather than the expiry the session started out with. The renewal
	// has already happened on the response being handled, so this must cost no request of its own —
	// hence no fetch stubbing here: a request would reach the network and fail the run.
	describe('Session expiry on activity', () => {
		type UmbTestResponseInterceptor = (
			response: Response,
			request: Request,
			options: unknown,
		) => Response | Promise<Response>;

		const SESSION_LENGTH_IN_SECONDS = 1200;

		let channel: BroadcastChannel;
		let peerMessages: Array<Record<string, unknown>>;
		let peerChannel: BroadcastChannel;

		beforeEach(() => {
			// The interceptor signals activity through a context it provides on the host, and context
			// requests only resolve for an attached host.
			document.body.appendChild(hostElement);

			channel = new BroadcastChannel('umb:auth');
			peerMessages = [];
			peerChannel = new BroadcastChannel('umb:auth');
			peerChannel.onmessage = (evt: MessageEvent) => peerMessages.push(evt.data);
		});

		afterEach(() => {
			channel.close();
			peerChannel.close();
			hostElement.remove();
		});

		function getLatestExpiresAt(): number | undefined {
			let expiresAt: number | undefined;
			context.session$.subscribe((session) => (expiresAt = session?.expiresAt)).unsubscribe();
			return expiresAt;
		}

		/** Configures a client that only records the response interceptors bound to it. */
		function collectResponseInterceptors(): Array<UmbTestResponseInterceptor> {
			const interceptors: Array<UmbTestResponseInterceptor> = [];
			const fakeClient = {
				buildUrl: () => '',
				getConfig: () => ({}),
				request: () => Promise.resolve({}) as never,
				interceptors: {
					request: { use: () => {} },
					response: { use: (fn: UmbTestResponseInterceptor) => interceptors.push(fn) },
				},
				setConfig: (config: Record<string, unknown>) => config,
			};

			context.configureClient(fakeClient as never);
			return interceptors;
		}

		async function establishSession(): Promise<number> {
			channel.postMessage({
				type: 'authorized',
				expiresIn: SESSION_LENGTH_IN_SECONDS,
				issuedAt: Math.floor(Date.now() / 1000),
			});
			await aTimeout(50);

			return getLatestExpiresAt()!;
		}

		async function receiveResponse(status: number): Promise<void> {
			let response = new Response(null, { status });
			const request = new Request('http://localhost/umbraco/management/api/v1/some-resource');
			for (const interceptor of collectResponseInterceptors()) {
				response = await interceptor(response, request, {});
			}
			await aTimeout(50);
		}

		// A session can be established before anything provides a modal manager — a peer tab
		// broadcasting one while this tab is still booting. Dismissing the timeout modal is a no-op
		// then, and must stay one rather than surfacing as a rejection nobody handles.
		it('does not float a rejection when a session arrives with no modal manager to dismiss', async () => {
			const rejections: Array<unknown> = [];
			const collectRejection = (event: PromiseRejectionEvent) => rejections.push(event.reason);
			window.addEventListener('unhandledrejection', collectRejection);

			try {
				await establishSession();
				await aTimeout(50);
			} finally {
				window.removeEventListener('unhandledrejection', collectRejection);
			}

			expect(rejections).to.be.empty;
		});

		it('slides the expiry forward when a request succeeds', async () => {
			const expiresAt = await establishSession();

			await receiveResponse(200);

			expect(getLatestExpiresAt()).to.be.greaterThan(expiresAt);
		});

		it('leaves the expiry alone when a request fails', async () => {
			const expiresAt = await establishSession();

			await receiveResponse(404);

			expect(getLatestExpiresAt()).to.equal(expiresAt);
		});

		// Peer tabs share the auth cookie, so the renewal applied to them as well — they just did not
		// make the request that triggered it.
		it('tells peer tabs about the slid expiry', async () => {
			const expiresAt = await establishSession();

			await receiveResponse(200);

			const sessionUpdates = peerMessages.filter((message) => message.type === 'sessionUpdate');
			expect(sessionUpdates).to.have.lengthOf(1);
			expect(sessionUpdates[0].expiresAt).to.equal(getLatestExpiresAt());
			expect(sessionUpdates[0].expiresAt).to.be.greaterThan(expiresAt);
		});

		// Activity re-issues the session for the lifetime it was issued for, so the window has to be
		// taken from that reported lifetime rather than from however much of it is left by the time the
		// report is applied.
		it('slides by the lifetime the session was issued for, not by what is left of it', async () => {
			const elapsedInSeconds = 600;
			channel.postMessage({
				type: 'authorized',
				expiresIn: SESSION_LENGTH_IN_SECONDS,
				issuedAt: Math.floor(Date.now() / 1000) - elapsedInSeconds,
			});
			await aTimeout(50);

			await receiveResponse(200);

			const [sessionUpdate] = peerMessages.filter((message) => message.type === 'sessionUpdate');
			expect(sessionUpdate?.sessionWindowInMs).to.equal(SESSION_LENGTH_IN_SECONDS * 1000);
			expect(getLatestExpiresAt()).to.be.greaterThan(
				Math.floor(Date.now() / 1000) + SESSION_LENGTH_IN_SECONDS - elapsedInSeconds,
			);
		});

		// A session whose expiry the server never reported gets no countdown at all (see the timeout
		// controller), and activity must not turn that into a guessed one.
		it('does not invent an expiry for a session that has none', async () => {
			channel.postMessage({ type: 'authorized', expiresIn: undefined, issuedAt: Math.floor(Date.now() / 1000) });
			await aTimeout(50);

			await receiveResponse(200);

			expect(context.getIsAuthorized()).to.be.true;
			expect(getLatestExpiresAt()).to.be.undefined;
			expect(peerMessages.filter((message) => message.type === 'sessionUpdate')).to.be.empty;
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
				// Only intercept the endpoints under test. The stub is global, so anything else — most
				// importantly the test runner's own reporting requests — has to reach the real fetch,
				// otherwise the session never completes and the run times out instead of failing.
				if (!String(input).includes('/umbraco/management/api/')) {
					return originalFetch.call(window, input, init);
				}

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

			// `timeoutUtc` is nullable on the server. Authorized-with-unknown-expiry is a real state,
			// and it must not be turned into a guessed lifetime — see the timeout controller, which
			// schedules no countdown for it.
			it('authorizes without an expiry when the server reports no timeout', async () => {
				stubFetch(() => new Response(JSON.stringify({}), { status: 200 }));

				await context.setInitialState();

				expect(context.getIsAuthorized()).to.be.true;
				const session = getLatestSession();
				expect(session).to.not.be.undefined;
				expect(session!.expiresAt).to.be.undefined;
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

			// The server owns the expiry and slides it on every request, so a renewed expiry can only
			// be learned by reading it back. Deriving it locally from a remembered lifetime drifts
			// from the server and eventually discards a session the server still accepts.
			it('re-reads the configuration on every renewal', async () => {
				const timeoutUtc = new Date(Date.now() + 20 * 60 * 1000).toISOString();
				stubFetch((input) =>
					String(input).includes('keep-alive')
						? new Response(null, { status: 200 })
						: new Response(JSON.stringify({ timeoutUtc }), { status: 200 }),
				);

				expect(await context.keepAlive()).to.be.true;
				expect(fetchCalls).to.have.lengthOf(2);

				expect(await context.keepAlive()).to.be.true;
				expect(fetchCalls).to.have.lengthOf(4);
				expect(String(fetchCalls[2].input)).to.contain('keep-alive');
				expect(String(fetchCalls[3].input)).to.contain('user/current/configuration');
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

		// The server sends both an ended session and a signed-in user it denies access to the login page.
		describe('a request redirected to the login page', () => {
			let sessionAlive: boolean;

			beforeEach(async () => {
				// The interceptor provides its signaler on the host, and context requests only resolve for an attached host.
				document.body.appendChild(hostElement);
				sessionAlive = true;
				const timeoutUtc = new Date(Date.now() + 20 * 60 * 1000).toISOString();
				stubFetch(() =>
					sessionAlive
						? new Response(JSON.stringify({ timeoutUtc }), { status: 200 })
						: new Response(null, { status: 401 }),
				);
				await context.setInitialState();
				fetchCalls = [];
			});

			afterEach(() => {
				hostElement.remove();
			});

			async function receiveLoginRedirect(): Promise<Response> {
				const interceptors: Array<(response: Response, request: Request, options: unknown) => Promise<Response>> = [];
				context.configureClient({
					getConfig: () => ({}),
					interceptors: {
						request: { use: () => {} },
						response: { use: (fn: never) => interceptors.push(fn) },
					},
					setConfig: () => ({}),
				} as never);
				await aTimeout(0);

				let response = new Response('<!doctype html><title>Log in</title>', { status: 200 });
				Object.defineProperties(response, {
					redirected: { value: true },
					url: { value: 'http://localhost/umbraco/login?ReturnUrl=%2F' },
				});
				const request = new Request('http://localhost/umbraco/my-extension/settings', { method: 'POST' });
				for (const interceptor of interceptors) {
					response = await interceptor(response, request, {});
				}
				return response;
			}

			it('answers with a 403 and keeps the session while the server still accepts it', async () => {
				const response = await receiveLoginRedirect();

				expect(response.status).to.equal(403);
				expect(context.getIsAuthorized()).to.be.true;
				expect(String(fetchCalls[0].input)).to.contain('keep-alive');
			});

			it('ends the session once the server no longer accepts it', async () => {
				sessionAlive = false;

				const response = await receiveLoginRedirect();

				expect(response.status).to.equal(401);
				expect(context.getIsAuthorized()).to.be.false;
			});
		});
	});

	describe('Login URL construction', () => {
		let originalOpen: typeof window.open;
		let openedWindows: Array<{ url: string; target?: string; features?: string }>;

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

		// The allowlist itself is covered exhaustively in returnable-route.function.test.ts, which can
		// vary the path without navigating. Here we only assert that the decision is wired in: the test
		// runner's own path is not a back office route, so no returnUrl is carried.
		it('omits returnUrl when the current location is not a back office route', async () => {
			await context.makeAuthorizationRequest('Google', false);

			const url = new URL(openedWindows[0].url);
			expect(url.searchParams.get('returnUrl')).to.be.null;
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

	describe('Sign-in by another user', () => {
		const realFetch = window.fetch;
		let channel: BroadcastChannel;
		let userCalls: number;
		// Answers GET /user/current with the user behind the session cookie, or with the given failure
		let respondToUserCall: () => Promise<Response>;
		let userChanges: number;

		const userResponse = (id: string) =>
			Promise.resolve(
				new Response(JSON.stringify({ id }), { status: 200, headers: { 'Content-Type': 'application/json' } }),
			);
		const signedInAs = (id: string) => () => userResponse(id);
		const signInElsewhere = () =>
			channel.postMessage({ type: 'authorized', expiresIn: 300, issuedAt: Math.floor(Date.now() / 1000) });

		beforeEach(async () => {
			document.body.appendChild(hostElement);
			userCalls = 0;
			respondToUserCall = signedInAs('admin');
			window.fetch = ((input: RequestInfo | URL, init?: RequestInit) => {
				const url = input instanceof Request ? input.url : input.toString();
				// The test runner's own requests have to reach the real fetch
				if (!url.includes('/umbraco/management/api/')) return realFetch.call(window, input, init);
				if (url.endsWith('/user/current')) {
					userCalls++;
					return respondToUserCall();
				}
				// The keep-alive, and the configuration it reads the renewed expiry from
				const timeoutUtc = new Date(Date.now() + 300_000).toISOString();
				return Promise.resolve(new Response(JSON.stringify({ timeoutUtc }), { status: 200 }));
			}) as typeof window.fetch;
			userChanges = 0;
			context.userChanged.subscribe(() => userChanges++);

			// The page has loaded the backoffice for the admin
			channel = new BroadcastChannel('umb:auth');
			signInElsewhere();
			await waitUntil(() => context.getIsAuthorized());
			context.bindUser('admin');
		});

		afterEach(() => {
			window.fetch = realFetch;
			channel.close();
			hostElement.remove();
		});

		it('holds a sign-in until the signed-in user is known', async () => {
			let answer: (response: Response) => void = () => {};
			respondToUserCall = () => new Promise<Response>((resolve) => (answer = resolve));
			context.timeOut();

			signInElsewhere();
			await waitUntil(() => userCalls === 1);
			await aTimeout(50);
			expect(context.getIsAuthorized(), 'nothing may resume before the user is known').to.be.false;

			answer(await userResponse('admin'));
			await waitUntil(() => context.getIsAuthorized());
			expect(userChanges).to.equal(0);
		});

		it('resumes the page when the same user signs in again', async () => {
			context.timeOut();

			signInElsewhere();

			await waitUntil(() => context.getIsAuthorized());
			expect(userChanges).to.equal(0);
		});

		it('does not resume the page for a different user, and reports the change', async () => {
			respondToUserCall = signedInAs('editor');
			context.timeOut();

			signInElsewhere();

			await waitUntil(() => userChanges === 1);
			await aTimeout(50);
			expect(context.getIsAuthorized()).to.be.false;
		});

		it('reports the change again for each later sign-in, while the page has not started over', async () => {
			respondToUserCall = signedInAs('editor');
			context.timeOut();
			signInElsewhere();
			await waitUntil(() => userChanges === 1);

			signInElsewhere();

			await waitUntil(() => userChanges === 2);
			expect(context.getIsAuthorized()).to.be.false;
		});

		it('reports the change when a different user signs in while this page still holds a session', async () => {
			respondToUserCall = signedInAs('editor');

			signInElsewhere();

			await waitUntil(() => userChanges === 1);
		});

		it('holds a session another tab re-establishes after this one ended, until the user is known', async () => {
			respondToUserCall = signedInAs('editor');
			context.timeOut();

			channel.postMessage({
				type: 'sessionUpdate',
				expiresAt: Math.floor(Date.now() / 1000) + 240,
				sessionWindowInMs: 240_000,
			});

			await waitUntil(() => userChanges === 1);
			expect(context.getIsAuthorized()).to.be.false;
		});

		it('holds a session the server still has after this page ended its own, until the user is known', async () => {
			respondToUserCall = signedInAs('editor');
			context.timeOut();

			const renewed = await context.keepAlive();

			expect(renewed).to.be.false;
			expect(userChanges).to.equal(1);
			expect(context.getIsAuthorized()).to.be.false;
		});

		it('resumes the page from a session the server still has for the same user', async () => {
			context.timeOut();

			const renewed = await context.keepAlive();

			expect(renewed).to.be.true;
			expect(userChanges).to.equal(0);
			expect(context.getIsAuthorized()).to.be.true;
		});

		it('renews the session in hand without looking up the user', async () => {
			expect(await context.keepAlive()).to.be.true;
			expect(userCalls).to.equal(0);
		});

		it('retries a failed lookup of the signed-in user before deciding', async () => {
			let failures = 1;
			respondToUserCall = () =>
				failures-- > 0 ? Promise.resolve(new Response(null, { status: 500 })) : userResponse('admin');
			context.timeOut();

			signInElsewhere();

			await waitUntil(() => context.getIsAuthorized(), undefined, { timeout: 5000 });
			expect(userCalls).to.equal(2);
			expect(userChanges).to.equal(0);
		});

		it('treats a signed-in user that cannot be determined as a different user', async function () {
			// Every lookup attempt fails, so this waits for all retries
			this.timeout(6000);
			respondToUserCall = () => Promise.reject(new TypeError('Failed to fetch'));
			context.timeOut();

			signInElsewhere();

			await waitUntil(() => userChanges === 1, undefined, { timeout: 5000 });
			expect(context.getIsAuthorized()).to.be.false;
		});

		it('reports the change when the page is bound to a different user', () => {
			context.bindUser('admin');
			expect(userChanges).to.equal(0);

			context.bindUser('editor');
			expect(userChanges).to.equal(1);
		});

		it('applies a sign-in straight away while the page has not loaded a user', async () => {
			// The lookup never answers, so only a context that does not wait for it can resume
			respondToUserCall = () => new Promise<Response>(() => {});
			const unboundContext = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', false);
			unboundContext.timeOut();

			signInElsewhere();

			await waitUntil(() => unboundContext.getIsAuthorized());
			unboundContext.destroy();
		});
	});
});
