import type { ManifestEntityAction, MetaEntityActionDeleteKind } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionDeleteWithRelationKind
	extends ManifestEntityAction<MetaEntityActionDeleteWithRelationKind> {
	type: 'entityAction';
	kind: 'deleteWithRelation';
}

export interface MetaEntityActionDeleteWithRelationKind extends MetaEntityActionDeleteKind {
	referenceRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityActionDeleteWithRelationKind: ManifestEntityActionDeleteWithRelationKind;
	}
}
