import type { UmbSegmentCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbSegmentCollectionDataSource = UmbCollectionDataSource<
	UmbSegmentCollectionItemModel,
	UmbSegmentCollectionFilterModel
>;

export interface UmbSegmentCollectionItemModel {
	entityType: string;
	unique: string;
	alias: string;
	name: string;
}
