import { IUmbAuth } from './auth.interface.js';
import { UmbAuthFlow } from './auth-flow.js';
import { UmbLoggedInUser } from './types.js';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbAuthContext extends UmbBaseController implements IUmbAuth {

	#currentUser = new UmbObjectState<UmbLoggedInUser | undefined>(undefined);
	readonly currentUser = this.#currentUser.asObservable();

	#isLoggedIn = new UmbBooleanState<boolean>(false);
	readonly isLoggedIn = this.#isLoggedIn.asObservable();
	readonly languageIsoCode = this.#currentUser.asObservablePart((user) => user?.languageIsoCode ?? 'en-us');

	#authFlow;

	constructor(host: UmbControllerHostElement, serverUrl: string, redirectUrl: string, bypassAuth: boolean) {
		super(host)
		if(bypassAuth) {
			this.#isLoggedIn.next(true);
		} else {
			this.#authFlow = new UmbAuthFlow(serverUrl, redirectUrl);
			this.observe(this.#authFlow.authorized, (isAuthorized) => {
				if (isAuthorized) {
					this.#isLoggedIn.next(true);
				} else {
					this.#isLoggedIn.next(false);
				}
			});
		}

		this.observe(this.isLoggedIn, (isLoggedIn) => {
			if (isLoggedIn) {
				this.fetchCurrentUser();
			}
		});
	}

	/**
	 * Initiates the login flow.
	 */
	login(): void {
		return this.#authFlow?.makeAuthorizationRequest();
	}

	isAuthorized() {
		return this.#authFlow?.isAuthorized() ?? true;
	}

	setInitialState(): Promise<void> {
		return this.#authFlow?.setInitialState() ?? Promise.resolve();
	}

	async fetchCurrentUser(): Promise<UmbLoggedInUser | undefined> {
		const { data } = await tryExecuteAndNotify(this._host, UserResource.getUserCurrent());

		this.#currentUser.next(data);

		return data;
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
		return this.#authFlow?.performWithFreshTokens() ?? Promise.resolve('bypass');
	}

	/**
	 * Signs the user out by removing any tokens from the browser.
	 */
	signOut(): Promise<void> {
		return this.#authFlow?.signOut() ?? Promise.resolve();
	}

	/**
	 * Checks if a user is the current user.
	 *
	 * @param userId The user id to check
	 * @returns True if the user is the current user, otherwise false
	 */
	async isUserCurrentUser(userId: string): Promise<boolean> {
		const currentUser = await firstValueFrom(this.currentUser);
		return currentUser?.id === userId;
	}
}
