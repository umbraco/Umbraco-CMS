import type { UmbPickerDataSource } from '../data-source/types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbPickerCollectionDataSourceTextFilterFeature {
	/** Whether text filtering is enabled for this collection data source. */
	enabled: boolean;
}

export interface UmbPickerCollectionDataSourceFeatures {
	/** Observable configuration for text filter support. */
	supportsTextFilter: Observable<UmbPickerCollectionDataSourceTextFilterFeature>;
}

export interface UmbPickerCollectionDataSource<CollectionItemType extends UmbItemModel = UmbItemModel>
	extends UmbPickerDataSource,
		UmbCollectionRepository<CollectionItemType>,
		UmbApi {
	collectionPickableFilter?: (item: CollectionItemType) => boolean;
	/** Feature toggles for the collection data source. Each feature is individually observable. */
	features?: UmbPickerCollectionDataSourceFeatures;
}
