import type { ManifestClass } from '.';
import { UmbItemStore, UmbStoreBase, UmbTreeStore } from '@umbraco-cms/backoffice/store';

export interface ManifestStore extends ManifestClass<UmbStoreBase> {
	type: 'store';
}

export interface ManifestTreeStore extends ManifestClass<UmbTreeStore> {
	type: 'treeStore';
}

export interface ManifestItemStore extends ManifestClass<UmbItemStore> {
	type: 'itemStore';
}
