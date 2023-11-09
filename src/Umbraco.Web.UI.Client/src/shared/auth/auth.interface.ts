import type { UmbLoggedInUser } from './types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbAuth {
	/**
	 * Initiates the login flow.
	 */
	login(): void;

	/**
	 * Initialise the auth flow.
	 */
	setInitialState(): Promise<void>;

	/**
	 * Checks if there is a token and it is still valid.
	 */
	checkAuthorization(): boolean;
	/**
	 * Gets the latest token from the Management API.
	 * If the token is expired, it will be refreshed.
	 *
	 * NB! The user may experience being redirected to the login screen if the token is expired.
	 *
	 * @example
	 * ```js
	 *   const token = await authContext.getLatestToken();
	 *   const result = await fetch('https://my-api.com', { headers: { Authorization: `Bearer ${token}` } });
	 * ```
	 *
	 * @returns The latest token from the Management API
	 */
	getLatestToken(): Promise<string>;

	/**
	 * Get the current user model of the current user.
	 */
	get currentUser(): Observable<UmbLoggedInUser | undefined>;

	/**
	 * Get the current user's language ISO code.
	 */
	languageIsoCode: Observable<string>;

	/**
	 * Make a server request for the current user and save the state
	 */
	fetchCurrentUser(): Promise<UmbLoggedInUser | undefined>;

	/**
	 * Signs the user out by removing any tokens from the browser.
	 */
	signOut(): Promise<void>;

	/**
	 * Check if the given user is the current user.
	 */
	isUserCurrentUser(userId: string): Promise<boolean>;
}
