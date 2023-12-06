import { type ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { type UmbTreeStore } from '@umbraco-cms/backoffice/tree';

export interface ManifestStore extends ManifestApi<UmbStoreBase> {
	type: 'store';
}
// TODO: TREE STORE TYPE PROBLEM: Provide a base tree item type?
export interface ManifestTreeStore extends ManifestApi<UmbTreeStore<any>> {
	type: 'treeStore';
}

export interface ManifestItemStore extends ManifestApi<UmbItemStore> {
	type: 'itemStore';
}
