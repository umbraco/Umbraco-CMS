import type { UmbCollectionMenuItemContext } from '../collection-menu-item-context.interface.js';
import { UMB_COLLECTION_MENU_ITEM_CONTEXT } from '../collection-menu-item.context.token.js';
import type { UmbCollectionItemModel } from '../../../types.js';
import type { ManifestCollectionMenuItem } from '../extension/types.js';
import { UMB_COLLECTION_MENU_CONTEXT } from '../../default/default-collection-menu.context.token.js';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { map } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbDefaultCollectionMenuItemContext<
		CollectionMenuItemType extends UmbCollectionItemModel = UmbCollectionItemModel,
	>
	extends UmbContextBase
	implements UmbCollectionMenuItemContext<CollectionMenuItemType>
{
	#manifest?: ManifestCollectionMenuItem;

	protected readonly _item = new UmbObjectState<CollectionMenuItemType | undefined>(undefined);
	readonly item = this._item.asObservable();

	#isSelectable = new UmbBooleanState(false);
	readonly isSelectable = this.#isSelectable.asObservable();

	#isSelectableContext = new UmbBooleanState(false);
	readonly isSelectableContext = this.#isSelectableContext.asObservable();

	#isSelected = new UmbBooleanState(false);
	readonly isSelected = this.#isSelected.asObservable();

	#collectionMenuContext?: typeof UMB_COLLECTION_MENU_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_COLLECTION_MENU_ITEM_CONTEXT);
		this.#consumeContexts();
	}

	async #consumeContexts() {
		this.consumeContext(UMB_COLLECTION_MENU_CONTEXT, (context) => {
			this.#collectionMenuContext = context;
			this.#observeIsSelectable();
			this.#observeIsSelected();
		});
	}

	public set manifest(manifest: ManifestCollectionMenuItem | undefined) {
		if (this.#manifest === manifest) return;
		this.#manifest = manifest;
	}
	public get manifest() {
		return this.#manifest;
	}

	public setItem(item: CollectionMenuItemType | undefined) {
		this._item.setValue(item);

		if (item) {
			this.#observeIsSelectable();
			this.#observeIsSelected();
		}
	}

	public select() {
		const unique = this.getItem()?.unique;
		if (!unique) throw new Error('Could not select. Unique is missing');
		this.#collectionMenuContext?.selection.select(unique);
	}

	public deselect() {
		const unique = this.getItem()?.unique;
		if (!unique) throw new Error('Could not deselect. Unique is missing');
		this.#collectionMenuContext?.selection.deselect(unique);
	}

	getItem() {
		return this._item.getValue();
	}

	#observeIsSelectable() {
		if (!this.#collectionMenuContext) return;
		const item = this.getItem();
		if (!item) return;

		this.observe(
			this.#collectionMenuContext.selection.selectable,
			(value) => {
				this.#isSelectableContext.setValue(value);

				// If the collection menu is selectable, check if this item is selectable
				if (value === true) {
					const isSelectable = this.#collectionMenuContext?.selectableFilter?.(item) ?? true;
					this.#isSelectable.setValue(isSelectable);
				}
			},
			'observeIsSelectable',
		);
	}

	#observeIsSelected() {
		if (!this.#collectionMenuContext) return;
		const unique = this.getItem()?.unique;
		if (!unique) return;

		this.observe(
			this.#collectionMenuContext.selection.selection.pipe(map((selection) => selection.includes(unique))),
			(isSelected) => {
				this.#isSelected.setValue(isSelected);
			},
			'observeIsSelected',
		);
	}
}
