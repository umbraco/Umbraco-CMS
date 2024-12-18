import type { ManifestAuthProvider } from './auth-provider.extension.js';

export type * from './auth-provider.extension.js';

/**
 * User login state that can be used to determine the current state of the user.
 * @example 'loggedIn'
 */
export type UmbUserLoginState = 'loggingIn' | 'loggedOut' | 'timedOut';

export interface UmbAuthProviderDefaultProps {
	/**
	 * The manifest for the registered provider.
	 */
	manifest?: ManifestAuthProvider;

	/**
	 * The current user login state.
	 */
	userLoginState?: UmbUserLoginState;

	/**
	 * Callback that is called when the user selects a provider.
	 * @param providerName The name of the provider that the user selected.
	 * @param loginHint The login hint to use for login if available.
	 * @deprecated Use the manifest parameter instead.
	 */
	onSubmit(providerName: string, loginHint?: string): void;

	/**
	 * Callback that is called when the user selects a provider.
	 * @param manifest The manifest of the provider that the user selected.
	 */
	onSubmit(manifest: ManifestAuthProvider, loginHint?: string): void;
}
