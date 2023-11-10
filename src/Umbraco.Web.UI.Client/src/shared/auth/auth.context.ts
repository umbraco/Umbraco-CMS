import { IUmbAuth } from './auth.context.interface.js';
import { UmbAuthFlow } from './auth-flow.js';
import { UMB_AUTH_CONTEXT } from './auth.context.token.js';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbAuthContext extends UmbBaseController implements IUmbAuth {
	#isAuthorized = new UmbBooleanState<boolean>(false);
	readonly isAuthorized = this.#isAuthorized.asObservable();

	#isBypassed = false;
	#backofficePath: string;

	#authFlow;

	constructor(host: UmbControllerHostElement, serverUrl: string, backofficePath: string, isBypassed: boolean) {
		super(host);
		this.#isBypassed = isBypassed;
		this.#backofficePath = backofficePath;

		this.#authFlow = new UmbAuthFlow(serverUrl, this.#getRedirectUrl());
		this.provideContext(UMB_AUTH_CONTEXT, this);
	}

	/**
	 * Initiates the login flow.
	 */
	login(): void {
		return this.#authFlow.makeAuthorizationRequest();
	}

	/**
	 * Checks if the user is authorized. If Authorization is bypassed, the user is always authorized.
	 * @returns {boolean} True if the user is authorized, otherwise false.
	 */
	getIsAuthorized() {
		if (this.#isBypassed) {
			this.#isAuthorized.next(true);
			return true;
		} else {
			const isAuthorized = this.#authFlow.isAuthorized();
			this.#isAuthorized.next(isAuthorized);
			return isAuthorized;
		}
	}

	/**
	 * Sets the initial state of the auth flow.
	 * @returns {Promise<void>}
	 */
	setInitialState(): Promise<void> {
		return this.#authFlow.setInitialState();
	}

	/**
	 * Gets the latest token from the Management API.
	 * If the token is expired, it will be refreshed.
	 *
	 * NB! The user may experience being redirected to the login screen if the token is expired.
	 *
	 * @returns The latest token from the Management API
	 */
	getLatestToken(): Promise<string> {
		return this.#authFlow.performWithFreshTokens();
	}

	/**
	 * Signs the user out by removing any tokens from the browser.
	 * @return {*}  {Promise<void>}
	 * @memberof UmbAuthContext
	 */
	signOut(): Promise<void> {
		return this.#authFlow.signOut();
	}

	#getRedirectUrl() {
		return `${window.location.origin}${this.#backofficePath}`;
	}
}
