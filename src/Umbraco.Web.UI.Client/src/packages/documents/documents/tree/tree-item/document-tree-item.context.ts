import type { UmbDocumentTreeItemModel, UmbDocumentTreeRootModel } from '../types.js';
import { UmbDocumentTreeItemDataResolver } from '../document-tree-item-data-resolver.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UmbAncestorsEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbEntityContentTypeEntityContext } from '@umbraco-cms/backoffice/content-type';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { ensureSlash } from '@umbraco-cms/backoffice/router';

export class UmbDocumentTreeItemContext extends UmbDefaultTreeItemContext<
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootModel
> {
	// TODO: Provide this together with the EntityContext, ideally this takes part via a extension-type [NL]
	#isTrashedContext = new UmbIsTrashedEntityContext(this);
	#ancestorsContext = new UmbAncestorsEntityContext(this);
	#entityContentTypeContext = new UmbEntityContentTypeEntityContext(this);
	#item = new UmbDocumentTreeItemDataResolver(this);

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

	// A collection is only browsed via its Collection view when the user can actually access it.
	// When the node is a "no access" ancestor of the user's start node, it must remain expandable
	// in the tree so the user can browse down to their start node.
	readonly #collapsibleCollection = mergeObservables(
		[this.hasCollection, this.noAccess],
		([hasCollection, noAccess]) => hasCollection && !noAccess,
	);

	/**
	 * Whether the collection replaces expansion for this item.
	 *
	 * Drilling into a collection needs somewhere to drill: the menu navigates to the Collection view by path, and a host
	 * that drills into items takes the user into it. A tree with neither can do no more than expand, so it keeps the
	 * expand caret and its children — the subtree would otherwise be unreachable.
	 */
	public readonly drillableCollection = mergeObservables(
		[this.#collapsibleCollection, this.isMenu, this.drillable],
		([collapsibleCollection, isMenu, drillable]) => collapsibleCollection && (isMenu || drillable),
	);

	override setIsMenu(isMenu: boolean) {
		super.setIsMenu(isMenu);
		if (isMenu) {
			this.observe(
				this.#collapsibleCollection,
				(collapsibleCollection) => {
					if (collapsibleCollection) {
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
					} else {
						this.removeUmbControllerByAlias('observeCollectionHasActiveDescendant');
					}
				},
				'_whenMenuObserveHasCollection',
			);
		}
	}

	constructor(host: UmbControllerHost) {
		super(host);

		this.observe(
			this.isTrashed,
			(isTrashed) => {
				this.#isTrashedContext.setIsTrashed(isTrashed);
			},
			null,
		);

		this.observe(
			this.ancestors,
			(ancestors) => {
				this.#ancestorsContext.setAncestors(ancestors);
			},
			null,
		);
	}

	public override setTreeItem(treeItem: UmbDocumentTreeItemModel | undefined) {
		super.setTreeItem(treeItem);
		this.#item.setData(treeItem);

		const documentTypeUnique = treeItem?.documentType.unique;

		this.#entityContentTypeContext.setEntityType(documentTypeUnique ? UMB_DOCUMENT_TYPE_ENTITY_TYPE : undefined);
		this.#entityContentTypeContext.setUnique(documentTypeUnique);
	}

	public getHasCollection() {
		return this.#item.getHasCollection();
	}

	public override showChildren() {
		if (this.#getDrillableCollection()) {
			this.#activateCollection();
			return;
		}
		super.showChildren();
	}

	public override hideChildren() {
		if (this.#getDrillableCollection()) {
			this.#activateCollection();
			return;
		}
		super.hideChildren();
	}

	// Collections cannot be expanded/collapsed. In a menu we navigate to the Collection view via the path;
	// elsewhere we ask the host to drill into the item, which only happens where the host declared that it does.
	#activateCollection() {
		if (this.getIsMenu()) {
			this.#openCollection();
		} else {
			this.open();
		}
	}

	#getCollapsibleCollection(): boolean {
		return this.#item.getHasCollection() && this.getTreeItem()?.noAccess !== true;
	}

	#getDrillableCollection(): boolean {
		return this.#getCollapsibleCollection() && (this.getIsMenu() || this.getDrillable());
	}

	#openCollection() {
		// open the collection view for this item:
		history.pushState(null, '', ensureSlash(this.getPath()) + '?openCollection=true');
	}
}

export { UmbDocumentTreeItemContext as api };
