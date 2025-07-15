import type {
	ManifestEntityBulkAction,
	MetaEntityBulkActionDefaultKind,
} from '@umbraco-cms/backoffice/extension-registry';

export interface ManifestEntityBulkActionDeleteKind extends ManifestEntityBulkAction<MetaEntityBulkActionDeleteKind> {
	type: 'entityBulkAction';
	kind: 'delete';
}

export interface MetaEntityBulkActionDeleteKind extends MetaEntityBulkActionDefaultKind {
	detailRepositoryAlias: string;
	itemRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityBulkActionDeleteKind: ManifestEntityBulkActionDeleteKind;
	}
}
