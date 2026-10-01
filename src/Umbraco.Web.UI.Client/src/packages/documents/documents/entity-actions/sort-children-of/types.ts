import type { UmbSortChildrenByFieldOption, UmbSortChildrenOfByFieldArgs } from '@umbraco-cms/backoffice/tree';

/**
 * Arguments for sorting the children of a Document by a single field on the server.
 */
export interface UmbSortChildrenOfDocumentByFieldArgs extends UmbSortChildrenOfByFieldArgs {
	/**
	 * The culture whose variant name to sort by. Omitted to sort by the invariant name.
	 */
	culture?: string;
}

/**
 * A field the children of a Document can be sorted by on the server.
 */
export interface UmbSortChildrenOfDocumentByFieldOption extends UmbSortChildrenByFieldOption {
	/**
	 * Whether the sort order this field produces depends on the culture sorted by.
	 */
	variesByCulture?: boolean;
}
