import { UmbTreeItemContextBase } from '../tree-item-base/index.js';
import type { ManifestTreeItem, UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultTreeItemContext<
	TreeItemType extends UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel,
	ManifestTreeItemType extends ManifestTreeItem = ManifestTreeItem,
> extends UmbTreeItemContextBase<TreeItemType, TreeRootType, ManifestTreeItemType> {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}

export default UmbDefaultTreeItemContext;
