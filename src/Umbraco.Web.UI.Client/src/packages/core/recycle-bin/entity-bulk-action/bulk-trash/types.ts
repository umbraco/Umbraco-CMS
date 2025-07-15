import type {
	ManifestEntityBulkAction,
	MetaEntityBulkActionDefaultKind,
} from '@umbraco-cms/backoffice/extension-registry';

export interface ManifestEntityBulkActionTrashKind extends ManifestEntityBulkAction<MetaEntityBulkActionTrashKind> {
	type: 'entityBulkAction';
	kind: 'trash';
}

export interface MetaEntityBulkActionTrashKind extends MetaEntityBulkActionDefaultKind {
	recycleBinRepositoryAlias: string;
	itemRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityBulkActionTrashKind: ManifestEntityBulkActionTrashKind;
	}
}
