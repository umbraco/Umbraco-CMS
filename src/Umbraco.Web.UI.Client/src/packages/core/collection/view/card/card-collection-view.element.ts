import { UMB_COLLECTION_CONTEXT } from '../../default/index.js';
import type { UmbCollectionItemModel } from '../../types.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-card-collection-view')
export class UmbCardCollectionViewElement extends UmbLitElement {
	@state()
	private _items: Array<UmbCollectionItemModel> = [];

	@state()
	private _selectable = false;

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _loading = false;

	@state()
	private _itemHrefs: Map<string, string> = new Map();

	@state()
	private _hasBulkActions = false;

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;

			this.observe(
				this.#collectionContext?.selection.selectable,
				(selectable) => (this._selectable = selectable ?? false),
				'umbCollectionSelectableObserver',
			);

			this.observe(
				this.#collectionContext?.selection.selection,
				(selection) => (this._selection = selection ?? []),
				'umbCollectionSelectionObserver',
			);

			this.observe(
				this.#collectionContext?.items,
				async (items) => {
					this._items = items ?? [];
					await this.#updateItemHrefs();
				},
				'umbCollectionItemsObserver',
			);
		});
	}

	#onSelect(item: UmbCollectionItemModel) {
		this.#collectionContext?.selection.select(item.unique ?? '');
	}

	#onDeselect(item: UmbCollectionItemModel) {
		this.#collectionContext?.selection.deselect(item.unique ?? '');
	}

	async #updateItemHrefs() {
		const hrefs = new Map<string, string>();
		for (const item of this._items) {
			const href = await this.#collectionContext?.requestItemHref?.(item);
			if (href && item.unique) {
				hrefs.set(item.unique, href);
			}
		}
		this._itemHrefs = hrefs;
	}

	override render() {
		if (this._loading) return nothing;
		return html`
			<div id="card-grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</div>
		`;
	}

	#renderItem(item: UmbCollectionItemModel) {
		const href = item.unique ? this._itemHrefs.get(item.unique) : undefined;
		return html` <umb-entity-collection-item-card
			.item=${item}
			href=${href ?? nothing}
			?selectable=${this._selectable}
			?select-only=${this._selection.length > 0}
			?selected=${this.#collectionContext?.selection.isSelected(item.unique)}
			@selected=${() => this.#onSelect(item)}
			@deselected=${() => this.#onDeselect(item)}>
			<umb-entity-actions-bundle
				slot="actions"
				.entityType=${item.entityType}
				.unique=${item.unique}></umb-entity-actions-bundle>
		</umb-entity-collection-item-card>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			#card-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
				gap: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbCardCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-card-collection-view': UmbCardCollectionViewElement;
	}
}
