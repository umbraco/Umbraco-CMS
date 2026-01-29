import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export interface UmbUserGroupCollectionFilterModel extends UmbCollectionFilterModel {
	/** @deprecated Use 'filter' property instead. Will be removed in version 19.0.0 */
	query?: string;
}
