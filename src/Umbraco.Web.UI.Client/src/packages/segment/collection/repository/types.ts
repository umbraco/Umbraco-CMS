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
	/**
	 * An optional list of culture codes that the segment applies to.
	 * If null or undefined, the segment applies to all cultures.
	 */
	cultures?: Array<string> | null;
}
