import type { ManifestCollectionAction, MetaCollectionAction } from '@umbraco-cms/backoffice/collection';

export interface ManifestCollectionActionEmptyRecycleBinKind extends ManifestCollectionAction {
	type: 'collectionAction';
	kind: 'emptyRecycleBin';
	meta: MetaCollectionActionEmptyRecycleBinKind;
}

export interface MetaCollectionActionEmptyRecycleBinKind extends MetaCollectionAction {
	recycleBinRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEmptyRecycleBinCollectionActionKind: ManifestCollectionActionEmptyRecycleBinKind;
	}
}
