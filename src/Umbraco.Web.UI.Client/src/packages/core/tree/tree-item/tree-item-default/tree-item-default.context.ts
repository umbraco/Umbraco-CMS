import { UmbTreeItemContextBase } from '../tree-item-base/index.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultTreeItemContext<
	TreeItemType extends UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel,
> extends UmbTreeItemContextBase<TreeItemType, TreeRootType> {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}

export default UmbDefaultTreeItemContext;
