import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollectionMenuItem extends ManifestElementAndApi<any, any> {
	type: 'collectionMenuItem';
	forEntityTypes: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbCollectionMenuItem: ManifestCollectionMenuItem;
	}
}
