import { UmbAuthContext } from '../auth.context.js';
import type { UmbModalAuthTimeoutConfig } from '../modals/umb-auth-timeout-modal.token.js';
import { UmbAuthSessionTimeoutController } from './auth-session-timeout.controller.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

@customElement('test-auth-session-timeout-host')
class UmbTestAuthSessionTimeoutHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbAuthSessionTimeoutController', () => {
	let hostElement: UmbTestAuthSessionTimeoutHostElement;
	let context: UmbAuthContext;
	let controller: UmbAuthSessionTimeoutController;
	let channel: BroadcastChannel;
	let openedModals: Array<UmbModalAuthTimeoutConfig>;
	let closedModalKeys: Array<string>;
	let timeOutCalls: number;
	const realDateNow = Date.now;

	beforeEach(() => {
		hostElement = new UmbTestAuthSessionTimeoutHostElement();
		document.body.appendChild(hostElement);

		openedModals = [];
		closedModalKeys = [];
		timeOutCalls = 0;

		const mockModalManager = {
			// getHostElement is required for the context consumer to accept the instance
			getHostElement: () => hostElement,
			open: (_host: unknown, _token: unknown, args: { data: UmbModalAuthTimeoutConfig }) => {
				openedModals.push(args.data);
				return { onSubmit: () => new Promise(() => {}) };
			},
			close: (key: string) => {
				closedModalKeys.push(key);
			},
		};
		const provider = new UmbContextProvider(
			hostElement,
			UMB_MODAL_MANAGER_CONTEXT,
			mockModalManager as unknown as typeof UMB_MODAL_MANAGER_CONTEXT.TYPE,
		);
		provider.hostConnected();

		context = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', false);
		context.timeOut = () => {
			timeOutCalls++;
		};

		// The controller is not instantiated by UmbAuthContext in test environments, so create it manually.
		controller = new UmbAuthSessionTimeoutController(context);

		channel = new BroadcastChannel('umb:auth');
	});

	afterEach(() => {
		Date.now = realDateNow;
		channel.close();
		controller.destroy();
		context.destroy();
		document.body.innerHTML = '';
	});

	/**
	 * Injects a session into the auth context via the cross-tab BroadcastChannel,
	 * the same way a peer tab would share a refreshed session.
	 * @param accessTokenExpiresInSeconds Seconds until the access token expires.
	 * @param sessionExpiresInSeconds Seconds until the full session (refresh token) expires.
	 */
	async function injectSession(accessTokenExpiresInSeconds: number, sessionExpiresInSeconds: number) {
		const now = Math.floor(Date.now() / 1000);
		channel.postMessage({
			type: 'sessionUpdate',
			accessTokenExpiresAt: now + accessTokenExpiresInSeconds,
			expiresAt: now + sessionExpiresInSeconds,
		});
		// Wait for the BroadcastChannel message to be delivered and observed
		await aTimeout(50);
	}

	it('opens the timeout modal when the session enters the warning zone', async () => {
		await injectSession(5, 10);

		expect(openedModals).to.have.lengthOf(1);
		expect(openedModals[0].remainingTimeInSeconds).to.be.greaterThan(0);
		expect(openedModals[0].remainingTimeInSeconds).to.be.at.most(10);
		expect(timeOutCalls).to.equal(0);
	});

	it('does not time out when "Stay logged in" successfully refreshes the session', async () => {
		context.validateToken = async () => true;
		await injectSession(5, 10);

		expect(openedModals).to.have.lengthOf(1);
		openedModals[0].onContinue();
		await aTimeout(10);

		expect(timeOutCalls).to.equal(0);
	});

	it('times out when "Stay logged in" fails to refresh the session', async () => {
		context.validateToken = async () => false;
		await injectSession(5, 10);

		expect(openedModals).to.have.lengthOf(1);
		openedModals[0].onContinue();
		await aTimeout(10);

		expect(timeOutCalls).to.equal(1);
	});

	it('times out instead of opening the modal when the warning timer fires after the session expired (e.g. after system sleep)', async function () {
		this.timeout(5000);

		// Session expires in 17s, warning buffer is 15s, so the warning timer is scheduled 2s out.
		await injectSession(5, 17);
		expect(openedModals).to.have.lengthOf(0);

		// Simulate system sleep / background-tab throttling: the wall clock jumps past the
		// session expiry while the scheduled timer has not fired yet.
		Date.now = () => realDateNow() + 60_000;

		// Wait for the real 2s timer to fire (with slack for slow CI agents).
		await aTimeout(2500);

		expect(openedModals).to.have.lengthOf(0);
		expect(timeOutCalls).to.equal(1);
	});
});
