import { UmbTreeItemContextBase } from '../tree-item-base/index.js';
import type { UmbUniqueTreeItemModel } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultTreeItemContext<
	TreeItemModelType extends UmbUniqueTreeItemModel,
> extends UmbTreeItemContextBase<TreeItemModelType> {
	constructor(host: UmbControllerHost) {
		super(host, (x: UmbUniqueTreeItemModel) => x.unique);
	}
}

export default UmbDefaultTreeItemContext;
