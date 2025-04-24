import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { MetaEntityActionSortChildrenOfKind } from '@umbraco-cms/backoffice/tree';

export interface ManifestEntityActionSortChildrenOfContentKind
	extends ManifestEntityAction<MetaEntityActionSortChildrenOfKind> {
	type: 'entityAction';
	kind: 'sortChildrenOfContent';
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityActionSortChildrenOfContentKind: ManifestEntityActionSortChildrenOfContentKind;
	}
}
