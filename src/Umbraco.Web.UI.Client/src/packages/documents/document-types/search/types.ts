import type { UmbDocumentTypeItemModel } from '../repository/types.js';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export interface UmbDocumentTypeSearchItemModel extends UmbDocumentTypeItemModel {
	href: string;
}

export interface UmbDocumentTypeSearchRequestArgs extends UmbSearchRequestArgs {
	elementTypesOnly?: boolean;
}
