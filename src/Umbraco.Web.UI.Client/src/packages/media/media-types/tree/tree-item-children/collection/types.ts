import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbMediaTypeTreeItemChildrenCollectionFilterModel extends UmbCollectionFilterModel {
	parent: UmbEntityModel;
}
