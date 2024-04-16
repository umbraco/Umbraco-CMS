import type { UmbDuplicateToRequestArgs } from '@umbraco-cms/backoffice/entity-action';

export interface UmbDuplicateDocumentRequestArgs extends UmbDuplicateToRequestArgs {
	relateToOriginal: boolean;
	includeDescendants: boolean;
}
