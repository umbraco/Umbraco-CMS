import { UmbElementItemDataResolver } from '../item/data-resolver/element-item-data-resolver.js';
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
	#item = new UmbElementItemDataResolver(this);

	readonly name = this.#item.name;
	readonly icon = this.#item.icon;
	readonly typeUnique = this.#item.typeUnique;
	readonly isDraft = this.#item.isDraft;
	readonly flags = this.#item.flags;

	// TODO: Move to API
	readonly isTrashed = this._treeItem.asObservablePart((item) => item?.isTrashed ?? false);

	constructor(host: UmbControllerHost) {
		super(host);

		this.observe(this.isTrashed, (isTrashed) => {
			this.#isTrashedContext.setIsTrashed(isTrashed);
		});
	}

	public override setTreeItem(treeItem: UmbElementTreeItemModel | undefined) {
		super.setTreeItem(treeItem);
		this.#item.setData(treeItem as never);
	}
}

export { UmbElementTreeItemContext as api };
