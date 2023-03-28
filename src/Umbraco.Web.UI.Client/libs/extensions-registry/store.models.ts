import type { ManifestClass } from './models';
import { UmbStoreBase, UmbTreeStore } from '@umbraco-cms/backoffice/store';

export interface ManifestStore extends ManifestClass<UmbStoreBase> {
	type: 'store';
}

export interface ManifestTreeStore extends ManifestClass<UmbTreeStore> {
	type: 'treeStore';
}
