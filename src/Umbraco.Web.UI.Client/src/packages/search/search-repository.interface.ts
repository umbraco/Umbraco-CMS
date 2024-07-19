import type { UmbSearchRequestArgs, UmbSearchResultItemModel } from './types.js';
import type { UmbRepositoryResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbSearchRepository<SearchResultItemType extends UmbSearchResultItemModel> {
	search(args: UmbSearchRequestArgs): Promise<UmbRepositoryResponse<UmbPagedModel<SearchResultItemType>>>;
}
