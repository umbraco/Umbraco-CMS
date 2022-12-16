import type { ManifestElement } from './models';

export interface ManifestExternalLoginProvider extends ManifestElement {
	type: 'externalLoginProvider';
	meta: MetaExternalLoginProvider;
}

export interface MetaExternalLoginProvider {
	label: string;
	pathname: string;
}
