import { UmbAppAuthController } from './app-auth.controller.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbUserLoginState } from '@umbraco-cms/backoffice/auth';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { Subject } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('test-app-auth-controller-host')
class UmbTestAppAuthControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbAppAuthController', () => {
	let hostElement: UmbTestAppAuthControllerHostElement;
	let provider: UmbContextProvider;
	let timeoutSignal: Subject<void>;
	let flows: Array<{ userLoginState: UmbUserLoginState; finish: (isAuthorized: boolean) => void }>;

	beforeEach(async () => {
		timeoutSignal = new Subject<void>();
		provider = new UmbContextProvider(document.body, UMB_AUTH_CONTEXT, {
			getHostElement: () => document.body,
			timeoutSignal: timeoutSignal.asObservable(),
		} as never);
		provider.hostConnected();

		hostElement = new UmbTestAppAuthControllerHostElement();
		document.body.appendChild(hostElement);
		const controller = new UmbAppAuthController(hostElement);

		// Each sign-in flow stays open until the test finishes it, as the sign-in overlay does
		flows = [];
		controller.makeAuthorizationRequest = (userLoginState: UmbUserLoginState = 'loggingIn') =>
			new Promise<boolean>((resolve) => flows.push({ userLoginState, finish: resolve }));

		await waitUntil(() => timeoutSignal.observed);
	});

	afterEach(() => {
		hostElement.remove();
		provider.destroy();
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
});
