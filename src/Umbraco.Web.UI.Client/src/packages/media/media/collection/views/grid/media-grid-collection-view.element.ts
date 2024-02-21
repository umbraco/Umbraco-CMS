import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from '../../types.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

@customElement('umb-media-grid-collection-view')
export class UmbMediaGridCollectionViewElement extends UmbLitElement {
	@state()
	private _items: Array<UmbMediaCollectionItemModel> = [];

	@state()
	private _loading = false;

	@state()
	private _selection: Array<string | null> = [];

	#collectionContext?: UmbDefaultCollectionContext<UmbMediaCollectionItemModel, UmbMediaCollectionFilterModel>;

	constructor() {
		super();
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			this.#observeCollectionContext();
		});
	}

	#observeCollectionContext() {
		if (!this.#collectionContext) return;

		this.observe(this.#collectionContext.items, (items) => (this._items = items), 'umbCollectionItemsObserver');

		this.observe(
			this.#collectionContext.selection.selection,
			(selection) => (this._selection = selection),
			'umbCollectionSelectionObserver',
		);
	}

	#onOpen(item: UmbMediaCollectionItemModel) {
		//TODO: Fix when we have dynamic routing
		history.pushState(null, '', 'section/media-management/workspace/media/edit/' + item.unique);
	}

	#onSelect(item: UmbMediaCollectionItemModel) {
		if (item.unique) {
			this.#collectionContext?.selection.select(item.unique);
		}
	}

	#onDeselect(item: UmbMediaCollectionItemModel) {
		if (item.unique) {
			this.#collectionContext?.selection.deselect(item.unique);
		}
	}

	#isSelected(item: UmbMediaCollectionItemModel) {
		return this.#collectionContext?.selection.isSelected(item.unique);
	}

	render() {
		if (this._loading) {
			return html`<div class="container"><uui-loader></uui-loader></div>`;
		}

		if (this._items.length === 0) {
			return html`<div class="container"><p>${this.localize.term('content_listViewNoItems')}</p></div>`;
		}

		return html`
			<div id="media-grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderCard(item),
				)}
			</div>
		`;
	}

	#renderCard(item: UmbMediaCollectionItemModel) {
		// TODO: Fix the file extension when media items have a file extension. [?]
		return html`
			<uui-card-media
				.name=${item.name ?? 'Unnamed Media'}
				selectable
				?select-only=${this._selection && this._selection.length > 0}
				?selected=${this.#isSelected(item)}
				@open=${() => this.#onOpen(item)}
				@selected=${() => this.#onSelect(item)}
				@deselected=${() => this.#onDeselect(item)}
				class="media-item"
				file-ext="${item.icon}">
				<!-- TODO: [LK] I'd like to indicate a busy state when bulk actions are triggered. -->
				<!-- <div class="container"><uui-loader></uui-loader></div> -->
			</uui-card-media>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			.container {
				display: flex;
				justify-content: center;
				align-items: center;
			}

			#media-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
				grid-template-rows: repeat(auto-fill, 200px);
				gap: var(--uui-size-space-5);
			}
		`,
	];
}

export default UmbMediaGridCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-grid-collection-view': UmbMediaGridCollectionViewElement;
	}
}
