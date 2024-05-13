import { UmbTreeItemContextBase } from '../tree-item-base/index.js';
import type { UmbTreeItemModel } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultTreeItemContext<
	TreeItemModelType extends UmbTreeItemModel,
> extends UmbTreeItemContextBase<TreeItemModelType> {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}

export default UmbDefaultTreeItemContext;
