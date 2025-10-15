import type { UmbPickerDataSource } from '../types.js';
import type { UmbSearchRepository, UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

export interface UmbPickerSearchableDataSource<
	SearchResultItemType extends UmbSearchResultItemModel = UmbSearchResultItemModel,
> extends UmbPickerDataSource,
		UmbSearchRepository<SearchResultItemType> {
	searchPickableFilter?: (searchItem: SearchResultItemType) => boolean;
}
