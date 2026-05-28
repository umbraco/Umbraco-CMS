import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { MetaEntityActionSortChildrenOfKind } from '@umbraco-cms/backoffice/tree';
import type { UmbItemDataResolverConstructor } from '@umbraco-cms/backoffice/entity-item';

export interface ManifestEntityActionSortChildrenOfContentKind extends ManifestEntityAction<MetaEntityActionSortChildrenOfContentKind> {
	type: 'entityAction';
	kind: 'sortChildrenOfContent';
}

export interface MetaEntityActionSortChildrenOfContentKind extends MetaEntityActionSortChildrenOfKind {
	itemDataResolver?: UmbItemDataResolverConstructor;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityActionSortChildrenOfContentKind: ManifestEntityActionSortChildrenOfContentKind;
	}
}
