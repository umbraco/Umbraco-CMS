import type { UmbSortChildrenOfByFieldArgs } from '@umbraco-cms/backoffice/tree';

/**
 * Arguments for sorting the children of a Document by a single field on the server.
 */
export interface UmbSortChildrenOfDocumentByFieldArgs extends UmbSortChildrenOfByFieldArgs {
	/**
	 * The culture to sort by. Defaults to the current backoffice culture when omitted.
	 */
	culture?: string;
}
