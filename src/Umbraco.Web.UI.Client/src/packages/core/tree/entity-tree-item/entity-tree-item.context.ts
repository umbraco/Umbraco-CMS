import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-base.context.js';
import type { UmbEntityTreeItemModel } from '../types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbEntityTreeItemContext extends UmbTreeItemContextBase<UmbEntityTreeItemModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, (x: UmbEntityTreeItemModel) => x.id);
	}
}
