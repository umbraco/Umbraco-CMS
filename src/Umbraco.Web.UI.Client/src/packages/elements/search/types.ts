import type { UmbElementItemModel } from '../item/repository/types.js';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbDocumentTypeEntityType } from '@umbraco-cms/backoffice/document-type';

export interface UmbElementSearchItemModel extends UmbElementItemModel {
	// TODO: [v18] Temporarily added `name` field back in, as the `UmbSearchResultItemModel` (and `UmbNamedEntityModel`) require it. Mirrors `UmbDocumentSearchItemModel`.
	name: string;
	href: string;
	ancestors: Array<UmbElementSearchAncestorModel>;
}

export interface UmbElementSearchRequestArgs extends UmbSearchRequestArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>;
	includeTrashed?: boolean;
	culture?: string | null;
}

export interface UmbElementSearchAncestorModel {
	unique: string;
	name: string;
}
