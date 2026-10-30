import { UmbAppAuthController } from './app-auth.controller.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbUserLoginState } from '@umbraco-cms/backoffice/auth';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { BehaviorSubject, Subject } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';

@customElement('test-app-auth-controller-host')
class UmbTestAppAuthControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbAppAuthController', () => {
	let hostElement: UmbTestAppAuthControllerHostElement;
	let provider: UmbContextProvider;
	let currentUserProvider: UmbContextProvider;
	let controller: UmbAppAuthController;
	let timeoutSignal: Subject<void>;
	let userChanged: Subject<void>;
	let loadedUser: BehaviorSubject<string | undefined>;
	let boundUsers: Array<string>;
	let flows: Array<{ userLoginState: UmbUserLoginState; finish: (isAuthorized: boolean) => void }>;

	beforeEach(async () => {
		timeoutSignal = new Subject<void>();
		userChanged = new Subject<void>();
		boundUsers = [];
		provider = new UmbContextProvider(document.body, UMB_AUTH_CONTEXT, {
			getHostElement: () => document.body,
			timeoutSignal: timeoutSignal.asObservable(),
			userChanged: userChanged.asObservable(),
			bindUser: (unique: string) => boundUsers.push(unique),
		} as never);
		provider.hostConnected();
		loadedUser = new BehaviorSubject<string | undefined>(undefined);
		currentUserProvider = new UmbContextProvider(document.body, UMB_CURRENT_USER_CONTEXT, {
			getHostElement: () => document.body,
			unique: loadedUser.asObservable(),
		} as never);
		currentUserProvider.hostConnected();

		hostElement = new UmbTestAppAuthControllerHostElement();
		document.body.appendChild(hostElement);
		controller = new UmbAppAuthController(hostElement);

		// Each sign-in flow stays open until the test finishes it, as the sign-in overlay does
		flows = [];
		controller.makeAuthorizationRequest = (userLoginState: UmbUserLoginState = 'loggingIn') =>
			new Promise<boolean>((resolve) => flows.push({ userLoginState, finish: resolve }));

		await waitUntil(() => timeoutSignal.observed);
	});

	afterEach(() => {
		controller.destroy();
		hostElement.remove();
		provider.destroy();
		currentUserProvider.destroy();
	});

	it('starts one timed-out sign-in flow per session end', async () => {
		timeoutSignal.next();
		timeoutSignal.next();

		expect(flows).to.have.lengthOf(1);
		expect(flows[0].userLoginState).to.equal('timedOut');

		// Once the user has signed in again, the next session end starts a new flow
		flows[0].finish(true);
		await aTimeout(0);
		timeoutSignal.next();

		expect(flows).to.have.lengthOf(2);
	});

	it('binds the page to the user it loads', async () => {
		loadedUser.next('admin');

		await waitUntil(() => boundUsers.length > 0);
		expect(boundUsers).to.deep.equal(['admin']);
	});

	it('starts the backoffice over when the session has passed to another user', async () => {
		let restarts = 0;
		controller.restartForNewUser = () => {
			restarts++;
		};
		await waitUntil(() => userChanged.observed);

		userChanged.next();

		expect(restarts).to.equal(1);
	});

	it('lets no unsaved-changes guard hold the page when it starts over for another user', () => {
		// A guard registered once the app has started, as an editor with unsaved changes does
		const guard = (event: Event) => event.preventDefault();
		window.addEventListener('beforeunload', guard);
		// Cancels the navigation before it unloads the page, so the test page stays open
		const { navigation } = window as unknown as { navigation: EventTarget };
		const stay = (event: Event) => event.preventDefault();
		navigation.addEventListener('navigate', stay);
		try {
			controller.restartForNewUser();

			const unload = new Event('beforeunload', { cancelable: true });
			window.dispatchEvent(unload);
			expect(unload.defaultPrevented).to.be.false;
		} finally {
			window.removeEventListener('beforeunload', guard);
			navigation.removeEventListener('navigate', stay);
		}
	});
});
