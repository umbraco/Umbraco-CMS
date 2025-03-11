import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbDataTypeTreeItemChildrenCollectionFilterModel extends UmbCollectionFilterModel {
	parent: UmbEntityModel;
}
