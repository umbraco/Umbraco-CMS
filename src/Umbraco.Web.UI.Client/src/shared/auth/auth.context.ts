import { IUmbAuth } from './auth.interface.js';
import { UmbAuthFlow } from './auth-flow.js';
import { UmbLoggedInUser } from './types.js';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { BehaviorSubject, firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbAuthContext implements IUmbAuth {
	#currentUser = new UmbObjectState<UmbLoggedInUser | undefined>(undefined);
	readonly currentUser = this.#currentUser.asObservable();
	readonly isLoggedIn = new BehaviorSubject<boolean>(false);
	readonly languageIsoCode = this.#currentUser.asObservablePart((user) => user?.languageIsoCode ?? 'en-us');

	#host;
	#authFlow;

	constructor(host: UmbControllerHostElement, serverUrl: string, redirectUrl: string) {
		this.#host = host;
		this.#authFlow = new UmbAuthFlow(serverUrl, redirectUrl);

		this.isLoggedIn.subscribe((isLoggedIn) => {
			if (isLoggedIn) {
				this.fetchCurrentUser();
			}
		});
	}

	/**
	 * Initiates the login flow.
	 */
	login(): void {
		return this.#authFlow.makeAuthorizationRequest();
	}

	isAuthorized() {
		return this.#authFlow.isAuthorized();
	}

	setInitialState(): Promise<void> {
		return this.#authFlow.setInitialState();
	}

	async fetchCurrentUser(): Promise<UmbLoggedInUser | undefined> {
		const { data } = await tryExecuteAndNotify(this.#host, UserResource.getUserCurrent());

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
		return this.#authFlow.performWithFreshTokens();
	}

	/**
	 * Signs the user out by removing any tokens from the browser.
	 */
	signOut(): Promise<void> {
		return this.#authFlow.signOut();
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
