import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { MetaEntityActionSortChildrenOfContentKind } from '@umbraco-cms/backoffice/content';
import type { UmbSortChildrenByFieldOption, UmbSortChildrenOfByFieldArgs } from '@umbraco-cms/backoffice/tree';

/**
 * Arguments for sorting the children of a Document by a single field on the server.
 */
export interface UmbSortChildrenOfDocumentByFieldArgs extends UmbSortChildrenOfByFieldArgs {
	/**
	 * The culture to sort by. Defaults to the current backoffice culture when omitted.
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

export interface ManifestEntityActionSortChildrenOfDocumentKind extends ManifestEntityAction<MetaEntityActionSortChildrenOfContentKind> {
	type: 'entityAction';
	kind: 'sortChildrenOfDocument';
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestEntityActionSortChildrenOfDocumentKind: ManifestEntityActionSortChildrenOfDocumentKind;
	}
}
