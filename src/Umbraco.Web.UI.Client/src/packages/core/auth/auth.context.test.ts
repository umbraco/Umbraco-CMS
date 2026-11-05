import { UmbAuthContext } from './auth.context.js';
import { createClient, createConfig } from '../backend-api/client/index.js';
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
		// Create context with test parameters
		context = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', false);
	});

	afterEach(() => {
		// Clean up context
		context.destroy();
	});

	describe('Public API', () => {
		it('has an isAuthorized property', () => {
			expect(context).to.have.property('isAuthorized');
		});

		it('has an isInitialized property', () => {
			expect(context).to.have.property('isInitialized');
		});

		// setInitialized() used to be driven from the core entry point. It now runs in the constructor,
		// so guard the property that makes that safe: #isInitialized is a ReplaySubject(1), meaning a
		// subscriber attaching after construction still receives the emission rather than hanging.
		// Reading the deprecated getter logs a deprecation warning here — expected, since the test
		// runner's origin resolves to 'unknown' rather than 'core' so the warning isn't suppressed.
		it('emits isInitialized to a subscriber that attaches after construction', async () => {
			let emitted = false;
			context.isInitialized.subscribe(() => {
				emitted = true;
			});
			await aTimeout(0);
			expect(emitted).to.be.true;
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

		it('has a validateToken method', () => {
			expect(context).to.have.property('validateToken').that.is.a('function');
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
				interceptors: { request: { use: () => {} }, response: { use: () => {} }, error: { use: () => {} } },
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
			// Should not throw when called with no session
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
			expect(url).to.contain('/umbraco/oauth_complete');
		});

		it('generates correct post-logout redirect URL', () => {
			const url = context.getPostLogoutRedirectUrl();
			expect(url).to.contain('/umbraco/logout');
		});
	});
	describe('Refresh failure handling', () => {
		let fetchCalls: Array<string>;
		let fetchResponder: () => Response;
		let channel: BroadcastChannel;
		const realFetch = window.fetch;

		const invalidGrantResponse = () =>
			new Response(JSON.stringify({ error: 'invalid_grant', error_description: 'The token is no longer valid.' }), {
				status: 400,
				headers: { 'Content-Type': 'application/json' },
			});

		// Resolves as soon as the session reference actually changes. The race tests below hold
		// the origin-wide `umb:token-refresh` Web Lock while they wait, and web-test-runner runs
		// test files concurrently against a single origin — so waiting on a fixed timeout there
		// would block unrelated test files for the whole duration.
		const nextSessionChange = () => {
			let unsubscribe = () => {};
			const changed = new Promise<void>((resolve) => {
				let emissions = 0;
				const subscription = context.session$.subscribe(() => {
					if (++emissions > 1) resolve();
				});
				unsubscribe = () => subscription.unsubscribe();
			});
			return changed.finally(() => unsubscribe());
		};

		beforeEach(() => {
			fetchCalls = [];
			window.fetch = ((input: RequestInfo | URL) => {
				fetchCalls.push(input.toString());
				return Promise.resolve(fetchResponder());
			}) as typeof window.fetch;
			channel = new BroadcastChannel('umb:auth');
		});

		afterEach(() => {
			window.fetch = realFetch;
			channel.close();
		});

		it('does not call /token again after a definitive invalid_grant failure', async () => {
			fetchResponder = invalidGrantResponse;

			expect(await context.validateToken()).to.be.false;
			expect(await context.validateToken()).to.be.false;

			expect(fetchCalls).to.have.lengthOf(1);
		});

		it('times the user out on a definitive invalid_grant failure', async () => {
			fetchResponder = invalidGrantResponse;

			// A peer tab establishes the session that is about to be rejected
			const now = Math.floor(Date.now() / 1000);
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 60, expiresAt: now + 240 });
			await aTimeout(50);

			let timeOutCalls = 0;
			context.timeOut = () => {
				timeOutCalls++;
			};

			await context.validateToken();

			expect(timeOutCalls).to.equal(1);
		});

		it('does not time the user out when there was no session to lose', async () => {
			fetchResponder = invalidGrantResponse;
			let timeOutCalls = 0;
			context.timeOut = () => {
				timeOutCalls++;
			};

			await context.setInitialState();

			expect(timeOutCalls).to.equal(0);
			expect(context.getIsAuthorized()).to.be.false;
		});

		it('retries /token after a transient network failure', async () => {
			fetchResponder = () => {
				throw new TypeError('Failed to fetch');
			};

			expect(await context.validateToken()).to.be.false;
			expect(await context.validateToken()).to.be.false;

			expect(fetchCalls).to.have.lengthOf(2);
		});

		it('attempts /token again once a new session is established', async () => {
			fetchResponder = invalidGrantResponse;
			await context.validateToken();
			expect(fetchCalls).to.have.lengthOf(1);

			// A peer tab (or completed re-authentication) establishes a new session
			const now = Math.floor(Date.now() / 1000);
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 60, expiresAt: now + 240 });
			await aTimeout(50);

			await context.validateToken();
			expect(fetchCalls).to.have.lengthOf(2);
		});

		it('does not time out a session established while an older refresh was still in flight', async () => {
			const now = Math.floor(Date.now() / 1000);

			// The session that is about to be rejected server-side
			const established = nextSessionChange();
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 60, expiresAt: now + 240 });
			await established;

			let resolveFetch!: (response: Response) => void;
			let fetchStarted!: () => void;
			const fetchInFlight = new Promise<void>((resolve) => {
				fetchStarted = resolve;
			});
			window.fetch = (() => {
				fetchStarted();
				return new Promise<Response>((resolve) => {
					resolveFetch = resolve;
				});
			}) as typeof window.fetch;

			const refreshPromise = context.validateToken();

			// Wait for /token to actually be in flight — the refresh queues behind a Web Lock,
			// so superseding the session before then would just make the queued caller skip it.
			await fetchInFlight;

			// A fresh sign-in completes and supersedes the session being refreshed
			const superseded = nextSessionChange();
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 600, expiresAt: now + 900 });
			await superseded;

			let timeOutCalls = 0;
			context.timeOut = () => {
				timeOutCalls++;
			};

			resolveFetch(invalidGrantResponse());

			// The caller must be told the token is usable: the session-timeout controller
			// times the user out itself when validateToken() reports failure, which would
			// reintroduce the very teardown this guard exists to prevent.
			expect(await refreshPromise).to.be.true;
			expect(timeOutCalls).to.equal(0);
			expect(context.getIsAuthorized()).to.be.true;
		});

		it('reports success when a transient failure raced a newly established session', async () => {
			const now = Math.floor(Date.now() / 1000);
			const established = nextSessionChange();
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 60, expiresAt: now + 240 });
			await established;

			let rejectFetch!: (error: Error) => void;
			let fetchStarted!: () => void;
			const fetchInFlight = new Promise<void>((resolve) => {
				fetchStarted = resolve;
			});
			window.fetch = (() => {
				fetchStarted();
				return new Promise<Response>((_, reject) => {
					rejectFetch = reject;
				});
			}) as typeof window.fetch;

			const refreshPromise = context.validateToken();
			await fetchInFlight;

			const superseded = nextSessionChange();
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 600, expiresAt: now + 900 });
			await superseded;

			rejectFetch(new TypeError('Failed to fetch'));

			expect(await refreshPromise).to.be.true;
			expect(context.getIsAuthorized()).to.be.true;
		});

		// The reordering puts the identity check ahead of the success branch, so a refresh that
		// succeeds for a session which has since been replaced must not overwrite the newer one.
		it('does not apply a successful refresh whose session was superseded', async () => {
			const now = Math.floor(Date.now() / 1000);

			const established = nextSessionChange();
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 60, expiresAt: now + 240 });
			await established;

			let resolveFetch!: (response: Response) => void;
			let fetchStarted!: () => void;
			const fetchInFlight = new Promise<void>((resolve) => {
				fetchStarted = resolve;
			});
			window.fetch = (() => {
				fetchStarted();
				return new Promise<Response>((resolve) => {
					resolveFetch = resolve;
				});
			}) as typeof window.fetch;

			const refreshPromise = context.validateToken();
			await fetchInFlight;

			const superseded = nextSessionChange();
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 600, expiresAt: now + 900 });
			await superseded;

			let latest: { accessTokenExpiresAt: number; expiresAt: number } | undefined;
			const subscription = context.session$.subscribe((session) => {
				latest = session;
			});

			// Succeeds, with timings that would visibly shorten the session if applied
			resolveFetch(
				new Response(JSON.stringify({ access_token: '[redacted]', expires_in: 60, token_type: 'Bearer' }), {
					status: 200,
					headers: { 'Content-Type': 'application/json' },
				}),
			);
			await refreshPromise;
			subscription.unsubscribe();

			expect(latest?.accessTokenExpiresAt, 'the newer session must survive a superseded success').to.equal(now + 600);
			expect(latest?.expiresAt).to.equal(now + 900);
		});

		// The cold-boot shape of the same race: no session in hand when the refresh starts, so the
		// `hadSession` guard already suppresses the timeout. What survives that guard is the
		// `#sessionDead` latch, which would be applied to the session the login just established.
		it('does not latch a newly established session as dead when a boot refresh is rejected', async () => {
			const now = Math.floor(Date.now() / 1000);

			let resolveFetch!: (response: Response) => void;
			let fetchStarted!: () => void;
			const fetchInFlight = new Promise<void>((resolve) => {
				fetchStarted = resolve;
			});
			window.fetch = ((input: RequestInfo | URL) => {
				fetchCalls.push(input.toString());
				fetchStarted();
				return new Promise<Response>((resolve) => {
					resolveFetch = resolve;
				});
			}) as typeof window.fetch;

			// Cold boot with a stale refresh cookie and no session yet
			const bootRefresh = context.validateToken();
			await fetchInFlight;

			// The interactive login completes and establishes a session
			const superseded = nextSessionChange();
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 600, expiresAt: now + 900 });
			await superseded;

			resolveFetch(invalidGrantResponse());
			await bootRefresh;

			// The session must still be usable: a later refresh has to reach the server rather
			// than being short-circuited by a latch that belongs to the superseded session.
			expect(context.getIsAuthorized()).to.be.true;
			const callsBefore = fetchCalls.length;
			window.fetch = ((input: RequestInfo | URL) => {
				fetchCalls.push(input.toString());
				return Promise.resolve(
					new Response(JSON.stringify({ access_token: '[redacted]', expires_in: 300, token_type: 'Bearer' }), {
						status: 200,
						headers: { 'Content-Type': 'application/json' },
					}),
				);
			}) as typeof window.fetch;

			expect(await context.validateToken()).to.be.true;
			expect(fetchCalls.length, 'a later refresh must still reach /token').to.be.greaterThan(callsBefore);
		});
	});

	describe('Session end', () => {
		const apiUrl = 'http://localhost/umbraco/management/api/v1/user/current';
		const realFetch = window.fetch;
		let fetchCalls: Array<string>;
		let channel: BroadcastChannel;
		// A token refresh succeeds, and API calls get a 401 until a refresh or a new sign-in has
		// happened, so a request that is handled as if the session were still active shows up here.
		let serverAcceptsApiCalls: boolean;

		const tokenCalls = () => fetchCalls.filter((url) => url.endsWith('/token')).length;

		const jsonResponse = (body: unknown) =>
			new Response(JSON.stringify(body), { status: 200, headers: { 'Content-Type': 'application/json' } });

		beforeEach(async () => {
			document.body.appendChild(hostElement);
			fetchCalls = [];
			serverAcceptsApiCalls = false;
			window.fetch = ((input: RequestInfo | URL) => {
				const url = input instanceof Request ? input.url : input.toString();
				fetchCalls.push(url);
				if (url.endsWith('/token')) {
					serverAcceptsApiCalls = true;
					return Promise.resolve(jsonResponse({ access_token: '[redacted]', expires_in: 300, token_type: 'Bearer' }));
				}
				if (url.endsWith('/revoke')) {
					// Held, so sign-out never reaches its redirect, which would navigate the test page away
					return new Promise<Response>(() => {});
				}
				return Promise.resolve(
					serverAcceptsApiCalls ? jsonResponse({ id: 'user' }) : new Response(null, { status: 401 }),
				);
			}) as typeof window.fetch;

			// A session whose access token has expired, as it has by the time the session times out
			channel = new BroadcastChannel('umb:auth');
			const now = Math.floor(Date.now() / 1000);
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now - 60, expiresAt: now + 240 });
			await waitUntil(() => context.getIsAuthorized());
		});

		afterEach(() => {
			window.fetch = realFetch;
			channel.close();
			hostElement.remove();
		});

		it('does not refresh the token for a request made after the session timed out', async () => {
			context.timeOut();

			await context.getLatestToken();

			expect(tokenCalls()).to.equal(0);
			expect(context.getIsAuthorized()).to.be.false;
		});

		it('does not refresh the token for a request made once sign-out has started', async () => {
			void context.signOut();

			await context.getLatestToken();

			expect(tokenCalls()).to.equal(0);
		});

		it('holds a request made after the session timed out until the user signs in again', async () => {
			const client = createClient(createConfig());
			context.configureClient(client);
			context.timeOut();

			let settled = false;
			const request = client
				.get({ url: '/umbraco/management/api/v1/user/current', security: [{ scheme: 'bearer', type: 'http' }] })
				.finally(() => {
					settled = true;
				});

			await waitUntil(() => fetchCalls.includes(apiUrl));
			await aTimeout(50);
			expect(settled, 'the request must wait for the next sign-in').to.be.false;
			expect(tokenCalls()).to.equal(0);
			expect(context.getIsAuthorized()).to.be.false;

			// The user signs in again at the overlay, and the sign-in popup announces the new session
			serverAcceptsApiCalls = true;
			channel.postMessage({ type: 'authorized', expiresIn: 300, issuedAt: Math.floor(Date.now() / 1000) });

			const { response } = await request;
			expect(response?.status).to.equal(200);
			expect(context.getIsAuthorized()).to.be.true;
			expect(tokenCalls()).to.equal(0);
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
			window.fetch = ((input: RequestInfo | URL) => {
				const url = input instanceof Request ? input.url : input.toString();
				if (url.endsWith('/user/current')) {
					userCalls++;
					return respondToUserCall();
				}
				return Promise.resolve(new Response(null, { status: 401 }));
			}) as typeof window.fetch;
			userChanges = 0;
			context.userChanged.subscribe(() => userChanges++);

			// The page has loaded the backoffice for the admin
			channel = new BroadcastChannel('umb:auth');
			const now = Math.floor(Date.now() / 1000);
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 60, expiresAt: now + 240 });
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

			const now = Math.floor(Date.now() / 1000);
			channel.postMessage({ type: 'sessionUpdate', accessTokenExpiresAt: now + 60, expiresAt: now + 240 });

			await waitUntil(() => userChanges === 1);
			expect(context.getIsAuthorized()).to.be.false;
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
