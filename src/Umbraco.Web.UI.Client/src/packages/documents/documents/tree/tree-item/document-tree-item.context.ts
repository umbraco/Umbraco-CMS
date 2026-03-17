import type { UmbDocumentTreeItemModel, UmbDocumentTreeRootModel } from '../types.js';
import { UmbDocumentItemDataResolver } from '../../item/index.js';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ITEM_PICKER_MODAL } from '@umbraco-cms/backoffice/collection';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UmbAncestorsEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbEntityContentTypeEntityContext } from '@umbraco-cms/backoffice/content-type';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import { mergeObservables, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { ensureSlash, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';

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

	#collectionPickerPath = new UmbStringState<string | undefined>(undefined);
	readonly collectionPickerPath = this.#collectionPickerPath.asObservable();

	#collectionPickerRoute?: UmbModalRouteRegistrationController<
		typeof UMB_COLLECTION_ITEM_PICKER_MODAL.DATA,
		typeof UMB_COLLECTION_ITEM_PICKER_MODAL.VALUE
	>;

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

		this.observe(this.hasCollection, (hasCollection) => {
			if (hasCollection) {
				this.#setupCollectionPickerRoute();
			} else {
				this.#collectionPickerRoute?.destroy();
				this.#collectionPickerRoute = undefined;
				this.#collectionPickerPath.setValue(undefined);
			}
		});
	}

	#setupCollectionPickerRoute() {
		this.#collectionPickerRoute = new UmbModalRouteRegistrationController(this, UMB_COLLECTION_ITEM_PICKER_MODAL)
			.addUniquePaths(['unique'])
			.onSetup(() => {
				const treeItem = this._treeItem.getValue();
				const unique = treeItem?.unique;
				if (!unique) return false;
				//const dataTypeId = treeItem?.documentType.collection?.unique ?? undefined;
				return {
					data: {
						collection: {
							alias: UMB_DOCUMENT_COLLECTION_ALIAS,
							filterArgs: { unique },
						},
						multiple: false,
					},
					value: { selection: [] },
				};
			})
			.onSubmit((value) => {
				if (!value?.selection?.length) return;
				for (const unique of value.selection) {
					this.treeContext?.selection.select(unique);
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				const treeItemUnique = this._treeItem.getValue()?.unique;
				this.#collectionPickerPath.setValue(treeItemUnique ? routeBuilder({ unique: treeItemUnique }) : undefined);
			});

		this.#collectionPickerRoute.setUniquePathValue('unique', this._treeItem.getValue()?.unique ?? '');
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
