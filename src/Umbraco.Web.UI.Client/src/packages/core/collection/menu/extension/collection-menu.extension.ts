import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollectionMenu extends ManifestElementAndApi<any, any> {
	type: 'collectionMenu';
	meta: MetaCollectionMenu;
}

export interface MetaCollectionMenu {
	collectionRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbCollectionMenu: ManifestCollectionMenu;
	}
}
