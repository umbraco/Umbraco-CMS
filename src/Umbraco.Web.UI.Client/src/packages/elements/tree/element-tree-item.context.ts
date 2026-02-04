import type { UmbElementTreeItemModel, UmbElementTreeRootModel } from './types.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementTreeItemContext extends UmbDefaultTreeItemContext<
	UmbElementTreeItemModel,
	UmbElementTreeRootModel
> {
	// TODO: Provide this together with the EntityContext, ideally this takes part via a extension-type [NL]
	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	// TODO: Move to API
	readonly isTrashed = this._treeItem.asObservablePart((item) => item?.isTrashed ?? false);

	constructor(host: UmbControllerHost) {
		super(host);

		this.observe(this.isTrashed, (isTrashed) => {
			this.#isTrashedContext.setIsTrashed(isTrashed);
		});
	}
}

export { UmbElementTreeItemContext as api };
