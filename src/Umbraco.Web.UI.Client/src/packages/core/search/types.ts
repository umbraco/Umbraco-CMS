import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

export type { UmbSearchDataSource } from './search-data-source.interface.js';
export type { UmbSearchRepository } from './search-repository.interface.js';

export type * from './extensions/types.js';
export type * from './global-search/types.js';

export interface UmbSearchResultItemModel extends UmbItemModel {
	href?: string;
}

export type UmbSearchRequestArgs = {
	query: string;
	searchFrom?: UmbEntityModel;
	paging?: UmbOffsetPaginationRequestModel;
};

export interface UmbSearchProvider<
	SearchResultItemType extends UmbSearchResultItemModel = UmbSearchResultItemModel,
	SearchRequestArgsType extends UmbSearchRequestArgs = UmbSearchRequestArgs,
> extends UmbApi {
	search(args: SearchRequestArgsType): Promise<UmbRepositoryResponse<UmbPagedModel<SearchResultItemType>>>;
}
