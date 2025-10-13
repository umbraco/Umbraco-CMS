import type { UmbPickerDataSource } from '../data-source/types.js';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbTreeItemModel, UmbTreeRepository, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbPickerTreeDataSource<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
> extends UmbPickerDataSource<UmbItemModel, TreeItemType>,
		UmbTreeRepository<TreeItemType, TreeRootType>,
		UmbApi {
	treePickableFilter?: (item: TreeItemType) => boolean;
}
