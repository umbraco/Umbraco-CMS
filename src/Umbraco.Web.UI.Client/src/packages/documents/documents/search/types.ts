import type { UmbDocumentItemModel } from '../item/types.js';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbDocumentTypeEntityType } from '@umbraco-cms/backoffice/document-type';

export interface UmbDocumentSearchItemModel extends UmbDocumentItemModel {
	// TODO: [v17] Temporarily added `name` field back in, as the `UmbSearchResultItemModel` (and `UmbNamedEntityModel`) require it. [LK]
	name: string;
	href: string;
}

export interface UmbDocumentSearchRequestArgs extends UmbSearchRequestArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>;
	includeTrashed?: boolean;
	culture?: string | null;
	dataTypeUnique?: string;
}
