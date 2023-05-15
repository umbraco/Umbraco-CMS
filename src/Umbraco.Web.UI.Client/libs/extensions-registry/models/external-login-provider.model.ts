import type { UmbExternalLoginProviderExtensionElement } from '../interfaces/external-login-provider-extension-element.interface';
import type { ManifestElement } from '@umbraco-cms/backoffice/extensions-api';

export interface ManifestExternalLoginProvider extends ManifestElement<UmbExternalLoginProviderExtensionElement> {
	type: 'externalLoginProvider';
	meta: MetaExternalLoginProvider;
}

export interface MetaExternalLoginProvider {
	label: string;
	pathname: string;
}
