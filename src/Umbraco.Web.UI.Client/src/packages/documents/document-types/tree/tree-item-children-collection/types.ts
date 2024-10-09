import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbDocumentTypeTreeItemChildrenCollectionFilterModel extends UmbCollectionFilterModel {
	parent: UmbEntityModel;
}
