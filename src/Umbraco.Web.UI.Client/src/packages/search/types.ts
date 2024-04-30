import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export type UmbSearchResultItemModel = {
	entityType: string;
	icon?: string | null;
	name: string;
	unique: string;
	href: string;
};

export type UmbSearchRequestArgs = {
	query: string;
};

export interface UmbSearchProvider<SearchResultItemType extends UmbSearchResultItemModel> extends UmbApi {
	search(args: UmbSearchRequestArgs): Promise<UmbRepositoryResponse<UmbPagedModel<SearchResultItemType>>>;
}
