import type { UmbPickerDataSource } from '../data-source/types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type {
	UmbTreeItemModel,
	UmbTreeRepository,
	UmbTreeRootModel,
	UmbTreeStartNode,
} from '@umbraco-cms/backoffice/tree';

export interface UmbPickerTreeDataSource<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
> extends UmbPickerDataSource,
		UmbTreeRepository<TreeItemType, TreeRootType>,
		UmbApi {
	requestTreeStartNode?: () => Promise<UmbTreeStartNode | undefined>;
	treePickableFilter?: (item: TreeItemType) => boolean;
}
