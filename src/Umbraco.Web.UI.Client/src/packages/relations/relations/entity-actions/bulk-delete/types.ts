import type { MetaEntityBulkActionDeleteKind } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

export interface ManifestEntityBulkActionDeleteWithRelationKind
	extends ManifestEntityBulkAction<MetaEntityBulkActionDeleteWithRelationKind> {
	type: 'entityBulkAction';
	kind: 'deleteWithRelation';
}

export interface MetaEntityBulkActionDeleteWithRelationKind extends MetaEntityBulkActionDeleteKind {
	referenceRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityBulkActionDeleteWithRelationKind: ManifestEntityBulkActionDeleteWithRelationKind;
	}
}
