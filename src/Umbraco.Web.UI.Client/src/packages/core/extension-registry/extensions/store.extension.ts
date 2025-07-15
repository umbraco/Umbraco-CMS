import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemStore } from '@umbraco-cms/backoffice/store';
import type { UmbTreeStore } from '@umbraco-cms/backoffice/tree';

export interface ManifestStore extends ManifestApi<any> {
	type: 'store';
}
// TODO: TREE STORE TYPE PROBLEM: Provide a base tree item type?
export interface ManifestTreeStore extends ManifestApi<UmbTreeStore<any>> {
	type: 'treeStore';
}

export interface ManifestItemStore extends ManifestApi<UmbItemStore<any>> {
	type: 'itemStore';
}

export type UmbStoreExtensions = ManifestStore | ManifestTreeStore | ManifestItemStore;

declare global {
	interface UmbExtensionManifestMap {
		UmbStoreExtensions: UmbStoreExtensions;
	}
}
