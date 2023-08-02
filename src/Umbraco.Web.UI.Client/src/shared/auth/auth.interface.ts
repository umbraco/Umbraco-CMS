import type { UmbLoggedInUser } from './types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface IUmbAuth {
	/**
	 * Initialise the auth flow.
	 */
	setInitialState(): Promise<void>;

	/**
	 * Get the current user's access token.
	 *
	 * @example
	 * ```js
	 *   const token = await auth.getAccessToken();
	 *   const result = await fetch('https://my-api.com', { headers: { Authorization: `Bearer ${token}` } });
	 * ```
	 */
	performWithFreshTokens(): Promise<string>;

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
	 * Sign out the current user.
	 */
	signOut(): Promise<void>;
}
