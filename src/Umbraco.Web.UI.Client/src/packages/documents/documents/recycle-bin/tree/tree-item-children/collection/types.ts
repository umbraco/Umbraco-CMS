import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbDocumentRecycleBinTreeItemChildrenCollectionFilterModel extends UmbCollectionFilterModel {
	parent: UmbEntityModel;
}
