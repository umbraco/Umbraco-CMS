import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export type { UmbSortChildrenOfRepository } from './sort-children-of-repository.interface.js';
export type { UmbSortChildrenOfDataSource } from './sort-children-of-data-source.interface.js';

export interface UmbSortChildrenOfArgs {
	unique: string | null;
	sorting: Array<{ unique: string; sortOrder: number }>;
}

export interface ManifestEntityActionSortChildrenOfKind
	extends ManifestEntityAction<MetaEntityActionSortChildrenOfKind> {
	type: 'entityAction';
	kind: 'sortChildrenOf';
}

export interface MetaEntityActionSortChildrenOfKind extends MetaEntityActionDefaultKind {
	sortChildrenOfRepositoryAlias: string;
	treeRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbSortChildrenOfEntityActionKind: ManifestEntityActionSortChildrenOfKind;
	}
}
