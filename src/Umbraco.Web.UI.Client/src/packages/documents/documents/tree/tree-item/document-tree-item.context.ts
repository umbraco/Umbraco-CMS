import type { UmbDocumentTreeItemModel, UmbDocumentTreeRootModel } from '../types.js';
import { UmbDocumentItemDataResolver } from '../../item/index.js';
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
				mergeObservables(
					[this.hasCollection, this.noAccess],
					([hasCollection, noAccess]) => hasCollection && !noAccess,
				),
				(isAccessibleCollection) => {
					if (isAccessibleCollection) {
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

		const documentTypeUnique = treeItem?.documentType.unique;

		this.#entityContentTypeContext.setEntityType(documentTypeUnique ? UMB_DOCUMENT_TYPE_ENTITY_TYPE : undefined);
		this.#entityContentTypeContext.setUnique(documentTypeUnique);
	}

	public getHasCollection() {
		return this.#item.getHasCollection();
	}

	public getNoAccess(): boolean {
		return this._treeItem.getValue()?.noAccess ?? false;
	}

	public override showChildren() {
		if (this.getIsMenu() && this.#item.getHasCollection() && !this.getNoAccess()) {
			// Collections cannot be expanded via a menu, instead we open the Collection for the user.
			// Exception: noAccess items (ancestors needed for navigating to a user start node) must remain
			// expandable so the user can browse to their start node within the collection.
			this.#openCollection();
			return;
		}
		super.showChildren();
	}

	public override hideChildren() {
		if (this.getIsMenu() && this.#item.getHasCollection() && !this.getNoAccess()) {
			// Collections in a menu will collapse when already showing children, and instead we open the Collection for the user.
			// Exception: noAccess items must remain expandable (see showChildren).
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
