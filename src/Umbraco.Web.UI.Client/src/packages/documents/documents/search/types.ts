import type { UmbDocumentItemModel } from '../item/types.js';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbDocumentTypeEntityType } from '@umbraco-cms/backoffice/document-type';

export interface UmbDocumentSearchItemModel extends UmbDocumentItemModel {
	href: string;
}

export interface UmbDocumentSearchRequestArgs extends UmbSearchRequestArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>;
	includeTrashed?: boolean;
}
