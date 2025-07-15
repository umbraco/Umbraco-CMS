import type { UmbBulkDuplicateToRequestArgs } from '@umbraco-cms/backoffice/entity-bulk-action';

export interface UmbBulkDuplicateToDocumentRequestArgs extends UmbBulkDuplicateToRequestArgs {
	relateToOriginal: boolean;
	includeDescendants: boolean;
}
