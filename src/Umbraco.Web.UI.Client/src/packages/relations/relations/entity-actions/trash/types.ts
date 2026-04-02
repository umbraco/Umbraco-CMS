import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { UmbItemDataResolverConstructor } from '@umbraco-cms/backoffice/entity-item';
import type { MetaEntityActionTrashKind } from '@umbraco-cms/backoffice/recycle-bin';

export interface ManifestEntityActionTrashWithRelationKind
	extends ManifestEntityAction<MetaEntityActionTrashWithRelationKind> {
	type: 'entityAction';
	kind: 'trashWithRelation';
}

export interface MetaEntityActionTrashWithRelationKind extends MetaEntityActionTrashKind {
	referenceRepositoryAlias: string;
	itemDataResolver?: UmbItemDataResolverConstructor;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityActionTrashWithRelationKind: ManifestEntityActionTrashWithRelationKind;
	}
}
