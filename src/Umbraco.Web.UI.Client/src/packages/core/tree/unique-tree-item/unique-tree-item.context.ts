import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-base.context.js';
import type { UmbUniqueTreeItemModel } from '../types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbUniqueTreeItemContext extends UmbTreeItemContextBase<UmbUniqueTreeItemModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, (x: UmbUniqueTreeItemModel) => x.unique);
	}
}
