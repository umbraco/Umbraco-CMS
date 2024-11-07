import type { ManifestCollectionAction } from '../../extensions/index.js';

export interface ManifestCollectionActionCreateKind extends ManifestCollectionAction {
	type: 'collectionAction';
	kind: 'create';
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionActionCreateKind: ManifestCollectionActionCreateKind;
	}
}
