import type { ManifestElement } from '.';

export interface ManifestExternalLoginProvider extends ManifestElement {
	type: 'externalLoginProvider';
	meta: MetaExternalLoginProvider;
}

export interface MetaExternalLoginProvider {
	label: string;
	pathname: string;
}
