import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionEmptyRecycleBinKind
	extends ManifestEntityAction<MetaEntityActionEmptyRecycleBinKind> {
	type: 'entityAction';
	kind: 'emptyRecycleBin';
}

export interface MetaEntityActionEmptyRecycleBinKind extends MetaEntityActionDefaultKind {
	recycleBinRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEmptyRecycleBinEntityActionKind: ManifestEntityActionEmptyRecycleBinKind;
	}
}
