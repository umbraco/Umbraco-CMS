import { UmbAppAuthController } from './app-auth.controller.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbAuthContext } from '@umbraco-cms/backoffice/auth';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

@customElement('test-app-auth-controller-host')
class UmbTestAppAuthControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/**
 * Two providers, so the cold-boot guard cannot take the single-provider shortcut (which navigates
 * away to the provider instead of opening the modal).
 */
const PROVIDER_ALIASES = ['Test.AuthProvider.One', 'Test.AuthProvider.Two'];

describe('UmbAppAuthController', () => {
	let hostElement: UmbTestAppAuthControllerHostElement;
	let context: UmbAuthContext;
	let controller: UmbAppAuthController;
	let replaceStateCalls: number;
	let openedLoginStates: Array<string>;
	const realReplaceState = history.replaceState;

	beforeEach(() => {
		hostElement = new UmbTestAppAuthControllerHostElement();
		document.body.appendChild(hostElement);

		replaceStateCalls = 0;
		openedLoginStates = [];
		history.replaceState = () => {
			replaceStateCalls++;
		};

		umbExtensionsRegistry.registerMany(
			PROVIDER_ALIASES.map((alias) => ({
				type: 'authProvider' as const,
				alias,
				name: alias,
				forProviderName: alias,
			})),
		);

		const mockModalManager = {
			getHostElement: () => hostElement,
			open: (_host: unknown, _token: unknown, args: { data: { userLoginState: string } }) => {
				openedLoginStates.push(args.data.userLoginState);
				return { onSubmit: () => Promise.resolve({ success: true }) };
			},
			close: () => {},
		};
		const provider = new UmbContextProvider(
			hostElement,
			UMB_MODAL_MANAGER_CONTEXT,
			mockModalManager as unknown as typeof UMB_MODAL_MANAGER_CONTEXT.TYPE,
		);
		provider.hostConnected();

		context = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', false);
		controller = new UmbAppAuthController(hostElement);
	});

	afterEach(() => {
		history.replaceState = realReplaceState;
		umbExtensionsRegistry.unregisterMany(PROVIDER_ALIASES);
		controller.destroy();
		context.destroy();
		document.body.innerHTML = '';
	});

	it('nudges the router when a cold-boot login succeeds', async () => {
		// The guard resolved false, so no route was rendered and the router slot is on its loading
		// fallback. A session arriving from a peer tab produces no history change of its own, so
		// without the nudge the tab sits on the spinner.
		expect(await controller.isAuthorized()).to.be.false;
		await aTimeout(50);

		expect(openedLoginStates).to.eql(['loggedOut']);
		expect(replaceStateCalls).to.equal(1);
	});

	// The timed-out route is already rendered behind the modal and may hold unsaved work, so it must
	// be left strictly alone — re-running the guard risks re-rendering it. Closing the modal is
	// enough, because the view was never taken away.
	it('does not touch the router when a timed-out session is re-authenticated', async () => {
		// Let the controller resolve the auth context and subscribe; timeoutSignal does not replay,
		// so an emission before it subscribes would simply be lost.
		await aTimeout(50);

		context.timeOut();
		// timeoutSignal is auditTime(1000), so the observer fires a beat later.
		await aTimeout(1200);

		expect(openedLoginStates).to.eql(['timedOut']);
		expect(replaceStateCalls).to.equal(0);
	});
});
