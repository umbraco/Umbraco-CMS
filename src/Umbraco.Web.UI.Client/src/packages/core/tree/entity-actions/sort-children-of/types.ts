import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { UmbDirectionType } from '@umbraco-cms/backoffice/utils';

export type { UmbSortChildrenOfRepository } from './sort-children-of-repository.interface.js';
export type { UmbSortChildrenOfDataSource } from './sort-children-of-data-source.interface.js';
export type * from './modal/types.js';

export interface UmbSortChildrenOfArgs {
	unique: string | null;
	sorting: Array<{ unique: string; sortOrder: number }>;
}

/**
 * Arguments for sorting the children of an entity by a single field on the server.
 */
export interface UmbSortChildrenOfByFieldArgs {
	unique: string | null;
	/**
	 * The field to sort by. The accepted values are defined by the entity's server endpoint
	 * (e.g. the Content sort fields for documents and media).
	 */
	field: string;
	direction: UmbDirectionType;
	/**
	 * The culture to sort by, for variant entities. Omitted or null for invariant sorting.
	 */
	culture?: string | null;
}

/**
 * An option shown in the "Sort by field" view of the sort children modal.
 */
export interface UmbSortChildrenByFieldOption {
	/**
	 * The field value sent to the server (see {@link UmbSortChildrenOfByFieldArgs.field}).
	 */
	value: string;
	/**
	 * The localized label shown to the user.
	 */
	label: string;
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
