import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-base.context.js';
import { UmbUniqueTreeItemModel } from '../types.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUniqueTreeItemContext<
	TreeItemModelType extends UmbUniqueTreeItemModel,
> extends UmbTreeItemContextBase<TreeItemModelType> {
	constructor(host: UmbControllerHost) {
		super(host, (x: UmbUniqueTreeItemModel) => x.unique);
	}
}
