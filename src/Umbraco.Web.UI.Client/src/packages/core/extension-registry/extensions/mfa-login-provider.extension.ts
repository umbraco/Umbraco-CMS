import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestMfaLoginProvider extends ManifestElement {
	type: 'mfaLoginProvider';

	/**
	 * The provider names that this provider is for.
	 * @examples 'Umbraco.GoogleAuthenticator'
	 */
	forProviderName: string;

	meta?: MetaMfaLoginProvider;
}

export interface MetaMfaLoginProvider {
	label?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbMfaLoginProvider: ManifestMfaLoginProvider;
	}
}
