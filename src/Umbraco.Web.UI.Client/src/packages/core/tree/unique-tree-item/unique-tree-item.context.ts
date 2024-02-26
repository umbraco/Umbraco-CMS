import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-context-base.js';
import type { UmbUniqueTreeItemModel } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUniqueTreeItemContext<
	TreeItemModelType extends UmbUniqueTreeItemModel,
> extends UmbTreeItemContextBase<TreeItemModelType> {
	constructor(host: UmbControllerHost) {
		super(host, (x: UmbUniqueTreeItemModel) => x.unique);
	}
}
