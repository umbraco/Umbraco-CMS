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
	 * The cultures the segment is limited to. When null or undefined, the segment is not limited to any culture.
	 * @deprecated Deprecated since v19. This property is temporary, as on the server. Scheduled for removal in Umbraco 20.
	 */
	cultures?: Array<string> | null;
}
