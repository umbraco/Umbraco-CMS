import type { UmbExternalLoginProviderExtensionElement } from '../interfaces/external-login-provider-extension-element.interface';
import type { ManifestElement } from 'src/libs/extension-api';

export interface ManifestExternalLoginProvider extends ManifestElement<UmbExternalLoginProviderExtensionElement> {
	type: 'externalLoginProvider';
	meta: MetaExternalLoginProvider;
}

export interface MetaExternalLoginProvider {
	label: string;
	pathname: string;
}
