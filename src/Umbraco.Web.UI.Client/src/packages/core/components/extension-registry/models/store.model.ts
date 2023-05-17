import type { ManifestClass } from 'src/libs/extension-api';
import { UmbItemStore, UmbStoreBase, UmbTreeStore } from 'src/libs/store';

export interface ManifestStore extends ManifestClass<UmbStoreBase> {
	type: 'store';
}

export interface ManifestTreeStore extends ManifestClass<UmbTreeStore> {
	type: 'treeStore';
}

export interface ManifestItemStore extends ManifestClass<UmbItemStore> {
	type: 'itemStore';
}
