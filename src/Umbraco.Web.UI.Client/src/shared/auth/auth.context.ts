import { IUmbAuth } from './auth.interface.js';
import { UmbAuthFlow } from './auth-flow.js';
import { UMB_AUTH_CONTEXT } from './auth.token.js';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbAuthContext extends UmbBaseController implements IUmbAuth {
	#isAuthorized = new UmbBooleanState<boolean>(false);
	readonly isAuthorized = this.#isAuthorized.asObservable();

	isBypassed = false;

	#authFlow;

	constructor(host: UmbControllerHostElement, serverUrl: string, redirectUrl: string, isBypassed: boolean) {
		super(host);
		this.isBypassed = isBypassed;
		this.#authFlow = new UmbAuthFlow(serverUrl, redirectUrl);
		this.provideContext(UMB_AUTH_CONTEXT, this);
	}

	/**
	 * Initiates the login flow.
	 */
	login(): void {
		return this.#authFlow.makeAuthorizationRequest();
	}

	/* TEMPORARY METHOD UNTIL RESPONSIBILITY IS MOVED TO CONTEXT */
	setLoggedIn(newValue: boolean): void {
		return this.#isAuthorized.next(newValue);
	}

	getIsAuthorized() {
		if (this.isBypassed) {
			this.#isAuthorized.next(true);
			return true;
		} else {
			const isAuthorized = this.#authFlow.isAuthorized();
			this.#isAuthorized.next(isAuthorized);
			return isAuthorized;
		}
	}

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
	 */
	signOut(): Promise<void> {
		return this.#authFlow.signOut();
	}
}
