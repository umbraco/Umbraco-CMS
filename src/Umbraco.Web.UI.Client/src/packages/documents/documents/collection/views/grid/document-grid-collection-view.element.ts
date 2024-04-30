import { getPropertyValueByAlias } from '../index.js';
import type { UmbCollectionColumnConfiguration } from '../../../../../core/collection/types.js';
import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../../types.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

@customElement('umb-document-grid-collection-view')
export class UmbDocumentGridCollectionViewElement extends UmbLitElement {
	@state()
	private _editDocumentPath = '';

	@state()
	private _items: Array<UmbDocumentCollectionItemModel> = [];

	@state()
	private _loading = false;

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _skip: number = 0;

	@state()
	private _userDefinedProperties?: Array<UmbCollectionColumnConfiguration>;

	#collectionContext?: UmbDefaultCollectionContext<UmbDocumentCollectionItemModel, UmbDocumentCollectionFilterModel>;

	constructor() {
		super();

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			this.#observeCollectionContext();
		});

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('document')
			.onSetup(() => {
				return { data: { entityType: 'document', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentPath = routeBuilder({});
			});
	}

	#observeCollectionContext() {
		if (!this.#collectionContext) return;

		this.observe(this.#collectionContext.loading, (loading) => (this._loading = loading), '_observeLoading');

		this.observe(
			this.#collectionContext.userDefinedProperties,
			(userDefinedProperties) => {
				this._userDefinedProperties = userDefinedProperties;
			},
			'_observeUserDefinedProperties',
		);

		this.observe(this.#collectionContext.items, (items) => (this._items = items), '_observeItems');

		this.observe(
			this.#collectionContext.selection.selection,
			(selection) => (this._selection = selection),
			'_observeSelection',
		);

		this.observe(
			this.#collectionContext.pagination.skip,
			(skip) => {
				this._skip = skip;
			},
			'_observePaginationSkip',
		);
	}

	#onOpen(event: Event, id: string) {
		event.preventDefault();
		event.stopPropagation();
		window.history.pushState(null, '', this._editDocumentPath + 'edit/' + id);
	}

	#onSelect(item: UmbDocumentCollectionItemModel) {
		this.#collectionContext?.selection.select(item.unique);
	}

	#onDeselect(item: UmbDocumentCollectionItemModel) {
		this.#collectionContext?.selection.deselect(item.unique);
	}

	#isSelected(item: UmbDocumentCollectionItemModel) {
		return this.#collectionContext?.selection.isSelected(item.unique);
	}

	render() {
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
			<div id="document-grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</div>
			${when(this._loading, () => html`<uui-loader-bar></uui-loader-bar>`)}
		`;
	}

	#renderItem(item: UmbDocumentCollectionItemModel) {
		return html`
			<uui-card-content-node
				.name=${item.name ?? 'Unnamed Document'}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#isSelected(item)}
				@open=${(event: Event) => this.#onOpen(event, item.unique ?? '')}
				@selected=${() => this.#onSelect(item)}
				@deselected=${() => this.#onDeselect(item)}>
				<umb-icon slot="icon" name=${item.icon}></umb-icon>
				${this.#renderState(item)} ${this.#renderProperties(item)}
			</uui-card-content-node>
		`;
	}

	#renderState(item: UmbDocumentCollectionItemModel) {
		switch (item.state) {
			case 'Published':
				return html`<uui-tag slot="tag" color="positive" look="secondary"
					>${this.localize.term('content_published')}</uui-tag
				>`;
			case 'PublishedPendingChanges':
				return html`<uui-tag slot="tag" color="warning" look="secondary"
					>${this.localize.term('content_publishedPendingChanges')}</uui-tag
				>`;
			case 'Draft':
				return html`<uui-tag slot="tag" color="default" look="secondary"
					>${this.localize.term('content_unpublished')}</uui-tag
				>`;
			case 'NotCreated':
				return html`<uui-tag slot="tag" color="danger" look="secondary"
					>${this.localize.term('content_notCreated')}</uui-tag
				>`;
			default:
				return html`<uui-tag slot="tag" color="danger" look="secondary">${fromCamelCase(item.state)}</uui-tag>`;
	}
	}

	#renderProperties(item: UmbDocumentCollectionItemModel) {
		if (!this._userDefinedProperties) return;
		return html`
			<ul>
				${repeat(
					this._userDefinedProperties,
					(column) => column.alias,
					(column) => html`<li><span>${column.header}:</span> ${getPropertyValueByAlias(item, column.alias)}</li>`,
				)}
			</ul>
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

			#document-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
				gap: var(--uui-size-space-4);
			}

			uui-card-content-node {
				width: 100%;
				height: 180px;
			}

			ul {
				list-style: none;
				padding-inline-start: 0px;
				margin: 0;
			}

			ul > li > span {
				font-weight: 700;
			}
		`,
	];
}

export default UmbDocumentGridCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-grid-collection-view': UmbDocumentGridCollectionViewElement;
	}
}
