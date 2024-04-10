import type { ManifestAuthProvider } from '../extension-registry/index.js';

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
	 */
	onSubmit: (providerName: string, loginHint?: string) => void;
}
