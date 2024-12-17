import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export type { UmbSearchDataSource } from './search-data-source.interface.js';
export type { UmbSearchRepository } from './search-repository.interface.js';

export type * from './extensions/types.js';

// TODO: lower requirement for search provider item type
export type UmbSearchResultItemModel = {
	entityType: string;
	icon?: string | null;
	name: string;
	unique: string;
	href: string;
};

export type UmbSearchRequestArgs = {
	query: string;
	searchFrom?: UmbEntityModel;
};

export interface UmbSearchProvider<
	SearchResultItemType extends UmbSearchResultItemModel = UmbSearchResultItemModel,
	SearchRequestArgsType extends UmbSearchRequestArgs = UmbSearchRequestArgs,
> extends UmbApi {
	search(args: SearchRequestArgsType): Promise<UmbRepositoryResponse<UmbPagedModel<SearchResultItemType>>>;
}
