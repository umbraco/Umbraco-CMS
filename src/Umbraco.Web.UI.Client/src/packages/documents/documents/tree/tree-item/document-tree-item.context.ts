import type { UmbDocumentTreeItemModel, UmbDocumentTreeRootModel } from '../types.js';
import { UmbDocumentItemDataResolver } from '../../item/index.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UmbAncestorsEntityContext } from '@umbraco-cms/backoffice/entity';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { ensureSlash } from '@umbraco-cms/backoffice/router';

export class UmbDocumentTreeItemContext extends UmbDefaultTreeItemContext<
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootModel
> {
	// TODO: Provide this together with the EntityContext, ideally this takes part via a extension-type [NL]
	#isTrashedContext = new UmbIsTrashedEntityContext(this);
	#ancestorsContext = new UmbAncestorsEntityContext(this);
	#item = new UmbDocumentItemDataResolver(this);

	readonly name = this.#item.name;
	readonly icon = this.#item.icon;
	readonly typeUnique = this.#item.typeUnique;
	readonly isDraft = this.#item.isDraft;
	readonly hasCollection = this.#item.hasCollection;
	public readonly hasChildrenOrCollection = mergeObservables(
		[this.hasCollection, this.hasChildren],
		([hasCollection, hasChildren]) => {
			return hasCollection || hasChildren;
		},
	);
	readonly flags = this.#item.flags;

	// TODO: Move to API
	readonly ancestors = this._treeItem.asObservablePart((item) => item?.ancestors ?? []);
	readonly isTrashed = this._treeItem.asObservablePart((item) => item?.isTrashed ?? false);
	readonly noAccess = this._treeItem.asObservablePart((item) => item?.noAccess ?? false);

	override setIsMenu(isMenu: boolean) {
		super.setIsMenu(isMenu);
		if (isMenu) {
			this.observe(
				this.hasCollection,
				(hasCollection) => {
					if (hasCollection) {
						this._treeItemChildrenManager.setTargetTakeSize(1, 1);

						this.observe(
							this.hasActiveDescendant,
							(active) => {
								if (active === false) {
									super.hideChildren();
								}
							},
							'observeCollectionHasActiveDescendant',
						);
					}
				},
				'_whenMenuObserveHasCollection',
			);
		}
	}

	constructor(host: UmbControllerHost) {
		super(host);

		this.observe(this.isTrashed, (isTrashed) => {
			this.#isTrashedContext.setIsTrashed(isTrashed);
		});

		this.observe(this.ancestors, (ancestors) => {
			this.#ancestorsContext.setAncestors(ancestors);
		});
	}

	public override setTreeItem(treeItem: UmbDocumentTreeItemModel | undefined) {
		super.setTreeItem(treeItem);
		this.#item.setData(treeItem);
	}

	public getHasCollection() {
		return this.#item.getHasCollection();
	}

	public override showChildren() {
		if (this.getIsMenu() && this.#item.getHasCollection()) {
			// Collections cannot be expanded via a menu, instead we open the Collection for the user.
			this.#openCollection();
			return;
		}
		super.showChildren();
	}

	public override hideChildren() {
		if (this.getIsMenu() && this.#item.getHasCollection()) {
			// Collections in a menu will collapse when already showing children, and instead we open the Collection for the user.
			this.#openCollection();
		}
		super.hideChildren();
	}

	#openCollection() {
		// open the collection view for this item:
		history.pushState(null, '', ensureSlash(this.getPath()) + '?openCollection=true');
	}
}

export { UmbDocumentTreeItemContext as api };
