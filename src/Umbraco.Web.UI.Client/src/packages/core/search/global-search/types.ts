import type { UmbSearchProvider, UmbSearchRequestArgs, UmbSearchResultItemModel } from '../types.js';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbGlobalSearchApi<
	SearchResultItemType extends UmbSearchResultItemModel = UmbSearchResultItemModel,
	SearchRequestArgsType extends UmbSearchRequestArgs = UmbSearchRequestArgs,
> extends UmbSearchProvider<SearchResultItemType, SearchRequestArgsType> {}

export type * from './global-search.extension.js';
