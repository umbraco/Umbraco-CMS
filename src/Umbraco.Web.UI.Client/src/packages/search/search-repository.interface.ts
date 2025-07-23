import type { UmbSearchRequestArgs, UmbSearchResultItemModel } from './types.js';
import type { UmbRepositoryResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbSearchRepository<
	SearchResultItemType extends UmbSearchResultItemModel,
	SearchRequestArgsType extends UmbSearchRequestArgs = UmbSearchRequestArgs,
> {
	search(args: SearchRequestArgsType): Promise<UmbRepositoryResponse<UmbPagedModel<SearchResultItemType>>>;
}
