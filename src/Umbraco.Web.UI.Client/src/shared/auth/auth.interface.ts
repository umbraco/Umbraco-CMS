export interface IUmbAuth {
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
	 * Sign out the current user.
	 */
	signOut(): Promise<void>;
}
