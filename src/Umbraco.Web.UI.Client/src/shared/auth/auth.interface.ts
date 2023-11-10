import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface IUmbAuth {
	isAuthorized: Observable<boolean>;

	setLoggedIn(loggedIn: boolean): void;

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
	getIsAuthorized(): boolean;

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
	 * Signs the user out by removing any tokens from the browser.
	 */
	signOut(): Promise<void>;
}
