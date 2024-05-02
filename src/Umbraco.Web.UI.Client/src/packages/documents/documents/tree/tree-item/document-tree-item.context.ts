import type { UmbDocumentTreeItemModel } from '../types.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';

export class UmbDocumentTreeItemContext extends UmbDefaultTreeItemContext<UmbDocumentTreeItemModel> {
	// TODO: Provide this together with the EntityContext, ideally this takes part via a extension-type [NL]
	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.observe(this.treeItem, (item) => {
			this.#isTrashedContext.setIsTrashed(item?.isTrashed || false);
		});
	}
}
