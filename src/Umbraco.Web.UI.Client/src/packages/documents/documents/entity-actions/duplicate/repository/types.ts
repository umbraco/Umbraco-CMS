import type { UmbDuplicateToRequestArgs } from '@umbraco-cms/backoffice/tree';

export interface UmbDuplicateDocumentRequestArgs extends UmbDuplicateToRequestArgs {
	relateToOriginal: boolean;
	includeDescendants: boolean;
}
