import type { UmbMediaTreeItemModel, UmbMediaTreeRootModel } from '../types.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';

export class UmbMediaTreeItemContext extends UmbDefaultTreeItemContext<UmbMediaTreeItemModel, UmbMediaTreeRootModel> {
	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.observe(this.treeItem, (item) => {
			this.#isTrashedContext.setIsTrashed(item?.isTrashed || false);
		});
	}
}

export { UmbMediaTreeItemContext as api };
