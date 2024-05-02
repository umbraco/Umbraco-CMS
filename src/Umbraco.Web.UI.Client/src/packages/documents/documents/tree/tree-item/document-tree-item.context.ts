import type { UmbDocumentTreeItemModel } from '../types.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityIsTrashedContext } from '@umbraco-cms/backoffice/recycle-bin';

export class UmbDocumentTreeItemContext extends UmbDefaultTreeItemContext<UmbDocumentTreeItemModel> {
	#isTrashedContext = new UmbEntityIsTrashedContext(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.observe(this.treeItem, (item) => {
			this.#isTrashedContext.setIsTrashed(item?.isTrashed || false);
		});
	}
}
