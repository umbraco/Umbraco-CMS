import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import type { MetaEntityBulkActionTrashKind } from '@umbraco-cms/backoffice/recycle-bin';

export interface ManifestEntityBulkActionTrashWithRelationKind
	extends ManifestEntityBulkAction<MetaEntityBulkActionTrashWithRelationKind> {
	type: 'entityBulkAction';
	kind: 'trashWithRelation';
}

export interface MetaEntityBulkActionTrashWithRelationKind extends MetaEntityBulkActionTrashKind {
	referenceRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityBulkActionTrashWithRelationKind: ManifestEntityBulkActionTrashWithRelationKind;
	}
}
