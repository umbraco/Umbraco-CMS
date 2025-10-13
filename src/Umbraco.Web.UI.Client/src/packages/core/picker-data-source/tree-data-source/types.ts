import type { UmbPickerDataSource } from '../data-source/types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbTreeItemModel, UmbTreeRepository, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbPickerTreeDataSource<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
> extends UmbPickerDataSource,
		UmbTreeRepository<TreeItemType, TreeRootType>,
		UmbApi {
	treePickableFilter?: (item: TreeItemType) => boolean;
}
