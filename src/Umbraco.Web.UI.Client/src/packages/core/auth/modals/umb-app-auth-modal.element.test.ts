import type { UmbAppAuthModalElement } from './umb-app-auth-modal.element.js';
import { UMB_AUTH_CONTEXT } from '../auth.context.token.js';
import type { UmbAuthViewElement } from '../components/umb-auth-view.element.js';
import './umb-app-auth-modal.element.js';
import { expect, waitUntil } from '@open-wc/testing';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

describe('UmbAppAuthModalElement', () => {
	let element: UmbAppAuthModalElement;
	let provider: UmbContextProvider;
	let isAuthorized: UmbBooleanState<boolean>;
	let setValueCalls: number;
	let submitCalls: number;

	beforeEach(async () => {
		isAuthorized = new UmbBooleanState<boolean>(false);
		provider = new UmbContextProvider(document.body, UMB_AUTH_CONTEXT, {
			getHostElement: () => document.body,
			isAuthorized: isAuthorized.asObservable(),
			getIsAuthorized: () => isAuthorized.getValue(),
			// A completed sign-in announces the new session before the sign-in request resolves
			makeAuthorizationRequest: async () => {
				isAuthorized.setValue(true);
			},
		} as never);
		provider.hostConnected();

		setValueCalls = 0;
		submitCalls = 0;
		let submitted = false;
		element = document.createElement('umb-app-auth-modal');
		element.data = { userLoginState: 'timedOut' };
		element.modalContext = {
			value: new UmbObjectState({}).asObservable(),
			setValue: () => {
				// A submitted modal is torn down, so its value can no longer be set
				if (submitted) throw new Error('The modal has already been submitted');
				setValueCalls++;
			},
			submit: () => {
				submitted = true;
				submitCalls++;
			},
		} as never;
		document.body.appendChild(element);
		await element.updateComplete;
	});

	afterEach(() => {
		element.remove();
		provider.destroy();
	});

	it('handles a sign-in success once', async () => {
		const view = element.shadowRoot!.querySelector<UmbAuthViewElement>('umb-auth-view')!;
		const errors: Array<unknown> = [];
		const realConsoleError = console.error;
		console.error = (...args: Array<unknown>) => errors.push(args);

		try {
			await view.props.onSubmit('Umbraco');
			await waitUntil(() => submitCalls > 0);
		} finally {
			console.error = realConsoleError;
		}

		expect(submitCalls).to.equal(1);
		expect(setValueCalls).to.equal(1);
		expect(errors, 'the sign-in view must not report an error').to.be.empty;
	});
});
