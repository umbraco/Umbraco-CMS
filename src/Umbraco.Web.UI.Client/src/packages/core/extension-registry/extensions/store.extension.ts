import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemStore } from '@umbraco-cms/backoffice/store';

export interface ManifestStore extends ManifestApi<any> {
	type: 'store';
}

export interface ManifestItemStore extends ManifestApi<UmbItemStore<any>> {
	type: 'itemStore';
}

export type UmbStoreExtensions = ManifestStore | ManifestItemStore;

declare global {
	interface UmbExtensionManifestMap {
		UmbStoreExtensions: UmbStoreExtensions;
	}
}
