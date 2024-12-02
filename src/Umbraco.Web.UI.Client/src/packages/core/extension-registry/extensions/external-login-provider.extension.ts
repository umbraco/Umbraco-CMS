import type { UmbExternalLoginProviderElement } from './external-login-provider-element.interface.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestExternalLoginProvider extends ManifestElement<UmbExternalLoginProviderElement> {
	type: 'externalLoginProvider';
	meta: MetaExternalLoginProvider;
}

export interface MetaExternalLoginProvider {
	label: string;
	pathname: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbExternalLoginProvider: ManifestExternalLoginProvider;
	}
}
