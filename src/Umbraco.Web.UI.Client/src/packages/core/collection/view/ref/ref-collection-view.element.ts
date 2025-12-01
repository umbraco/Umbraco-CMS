import { UMB_COLLECTION_CONTEXT, type UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-ref-collection-view')
export class UmbRefCollectionViewElement extends UmbLitElement {
	@state()
	private _items: Array<UmbCollectionItemModel> = [];

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _loading = false;

	@state()
	private _selectOnly: boolean | undefined;

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;

			this.observe(
				this.#collectionContext?.selection.selection,
				(selection) => (this._selection = selection ?? []),
				'umbCollectionSelectionObserver',
			);

			this.observe(
				this.#collectionContext?.selection.selectOnly,
				(selectOnly) => (this._selectOnly = selectOnly ?? undefined),
				'umbCollectionSelectOnlyObserver',
			);

			this.observe(
				this.#collectionContext?.items,
				(items) => (this._items = items ?? []),
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
		return html`<umb-entity-collection-item-ref
			.item=${item}
			selectable
			?select-only=${this._selection.length > 0 || this._selectOnly}
			?selected=${this.#collectionContext?.selection.isSelected(item.unique)}
			@selected=${() => this.#onSelect(item)}
			@deselected=${() => this.#onDeselect(item)}></umb-entity-collection-item-ref>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export { UmbRefCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-collection-view': UmbRefCollectionViewElement;
	}
}
