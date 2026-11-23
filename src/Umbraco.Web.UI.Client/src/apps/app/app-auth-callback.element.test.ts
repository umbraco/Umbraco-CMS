import { UmbAppAuthCallbackElement } from './app-auth-callback.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbAuthContext } from '@umbraco-cms/backoffice/auth';

describe('UmbAppAuthCallbackElement', () => {
	const originalClose = window.close;
	let closeCalls: number;
	let isAuthorized: UmbBasicState<boolean>;
	let provider: UmbContextProvider;

	beforeEach(() => {
		closeCalls = 0;
		window.close = () => closeCalls++;
		isAuthorized = new UmbBasicState<boolean>(true);
		const authContext = {
			getHostElement: () => document.body,
			isAuthorized: isAuthorized.asObservable(),
		} as unknown as UmbAuthContext;
		provider = new UmbContextProvider(document.body, UMB_AUTH_CONTEXT, authContext);
		provider.hostConnected();
	});

	afterEach(() => {
		provider.hostDisconnected();
		window.close = originalClose;
	});

	it('is defined with its own tag', () => {
		expect(document.createElement('umb-app-auth-callback')).to.be.instanceOf(UmbAppAuthCallbackElement);
	});

	// A sign-in lands here in any tab, also one that another page opened, which a script is allowed to
	// close. Only the window that opened a sign-in popup knows the popup is one.
	it('does not close the window it signed in', async () => {
		const element = await fixture<UmbAppAuthCallbackElement>(html`<umb-app-auth-callback></umb-app-auth-callback>`);
		await element.updateComplete;

		expect(closeCalls).to.equal(0);

		// Removing the element cancels its pending navigation, which would otherwise leave the test page.
		element.remove();
	});
});
