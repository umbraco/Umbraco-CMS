import type { ManifestClass } from './models';
import { UmbStoreBase, UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';

export interface ManifestStore extends ManifestClass<UmbStoreBase> {
	type: 'store';
}

export interface ManifestTreeStore extends ManifestClass<UmbEntityTreeStore> {
	type: 'treeStore';
}
