import type { ManifestCollectionAction } from '../../extensions/types.js';

export interface ManifestCollectionActionCreateKind extends ManifestCollectionAction {
	type: 'collectionAction';
	kind: 'create';
}
export interface UmbCollectionCreateOption {
	alias: string;
	label: string;
	icon?: string;
	href?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionActionCreateKind: ManifestCollectionActionCreateKind;
	}
}
