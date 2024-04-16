import type { UmbMediaTreeItemModel } from '../types.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaTreeItemContext extends UmbDefaultTreeItemContext<UmbMediaTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}

export { UmbMediaTreeItemContext as api };
