import type { UmbSearchRequestArgs, UmbSearchResultItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbSearchDataSourceConstructor<SearchResultItemType extends UmbSearchResultItemModel> {
	new (host: UmbControllerHost): UmbSearchDataSource<SearchResultItemType>;
}

export interface UmbSearchDataSource<SearchResultItemType extends UmbSearchResultItemModel> {
	search(args: UmbSearchRequestArgs): Promise<UmbDataSourceResponse<UmbPagedModel<SearchResultItemType>>>;
}
