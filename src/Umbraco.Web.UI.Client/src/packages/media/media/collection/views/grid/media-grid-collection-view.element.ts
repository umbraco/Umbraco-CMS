import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbMediaCollectionItemModel } from '../../types.js';
import type { UmbMediaCollectionContext } from '../../media-collection.context.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from '../../media-collection.context-token.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';

import '@umbraco-cms/backoffice/imaging';

@customElement('umb-media-grid-collection-view')
export class UmbMediaGridCollectionViewElement extends UmbLitElement {
	@state()
	private _editMediaPath = '';

	@state()
	private _items: Array<UmbMediaCollectionItemModel> = [];

	@state()
	private _loading = false;

	@state()
	private _selection: Array<string | null> = [];

	#collectionContext?: UmbMediaCollectionContext;

	constructor() {
		super();
		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			this.#observeCollectionContext();
		});

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media')
			.onSetup(() => {
				return { data: { entityType: 'media', preset: {} } };
			})
			.onReject(() => {
				this.#collectionContext?.requestCollection();
			})
			.onSubmit(() => {
				this.#collectionContext?.requestCollection();
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMediaPath = routeBuilder({});
			});
	}

	#observeCollectionContext() {
		if (!this.#collectionContext) return;

		this.observe(this.#collectionContext.loading, (loading) => (this._loading = loading), '_observeLoading');

		this.observe(this.#collectionContext.items, (items) => (this._items = items), '_observeItems');

		this.observe(
			this.#collectionContext.selection.selection,
			(selection) => (this._selection = selection),
			'_observeSelection',
		);
	}

	#onOpen(event: Event, unique: string) {
		event.preventDefault();
		event.stopPropagation();

		const url = this._editMediaPath + UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateLocal({ unique });
		window.history.pushState(null, '', url);
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

	override render() {
		return this._items.length === 0 ? this.#renderEmpty() : this.#renderItems();
	}

	#renderEmpty() {
		if (this._items.length > 0) return nothing;
		return html`
			<div class="container">
				${when(
					this._loading,
					() => html`<uui-loader></uui-loader>`,
					() => html`<p>${this.localize.term('content_listViewNoItems')}</p>`,
				)}
			</div>
		`;
	}

	#renderItems() {
		if (this._items.length === 0) return nothing;
		return html`
			<div id="media-grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</div>
			${when(this._loading, () => html`<uui-loader-bar></uui-loader-bar>`)}
		`;
	}

	#renderItem(item: UmbMediaCollectionItemModel) {
		return html`
			<uui-card-media
				.name=${item.name}
				selectable
				?select-only=${this._selection && this._selection.length > 0}
				?selected=${this.#isSelected(item)}
				@open=${(event: Event) => this.#onOpen(event, item.unique)}
				@selected=${() => this.#onSelect(item)}
				@deselected=${() => this.#onDeselect(item)}
				class="media-item">
				<umb-imaging-thumbnail unique=${item.unique} alt=${item.name} icon=${item.icon}></umb-imaging-thumbnail>
			</uui-card-media>
		`;
	}

	static override styles = [
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
				grid-auto-rows: 200px;
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
