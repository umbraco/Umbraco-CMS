import type { UmbPickerDataSource } from '../data-source/types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbPickerCollectionDataSourceTextFilterFeature {
	/** Whether text filtering is enabled for this collection data source. */
	enabled: boolean;
}

export interface UmbPickerCollectionDataSourceFeatures {
	/** Configuration for text filter support. */
	supportsTextFilter?: UmbPickerCollectionDataSourceTextFilterFeature;
}

export interface UmbPickerCollectionDataSource<CollectionItemType extends UmbItemModel = UmbItemModel>
	extends UmbPickerDataSource,
		UmbCollectionRepository<CollectionItemType>,
		UmbApi {
	collectionPickableFilter?: (item: CollectionItemType) => boolean;
	/** Optional feature toggles for the collection data source. */
	features?: UmbPickerCollectionDataSourceFeatures;
}
