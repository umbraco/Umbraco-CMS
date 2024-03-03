import { getPropertyValueByAlias } from '../index.js';
import type { UmbCollectionColumnConfiguration } from '../../../../../core/collection/types.js';
import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../../types.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

@customElement('umb-document-grid-collection-view')
export class UmbDocumentGridCollectionViewElement extends UmbLitElement {
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
	}

	#observeCollectionContext() {
		if (!this.#collectionContext) return;

		this.observe(
			this.#collectionContext.userDefinedProperties,
			(userDefinedProperties) => {
				this._userDefinedProperties = userDefinedProperties;
			},
			'umbCollectionUserDefinedPropertiesObserver',
		);

		this.observe(this.#collectionContext.items, (items) => (this._items = items), 'umbCollectionItemsObserver');

		this.observe(
			this.#collectionContext.selection.selection,
			(selection) => (this._selection = selection),
			'umbCollectionSelectionObserver',
		);

		this.observe(
			this.#collectionContext.pagination.skip,
			(skip) => {
				this._skip = skip;
			},
			'umbCollectionSkipObserver',
		);
	}

	// TODO: How should we handle url stuff? [?]
	#onOpen(id: string) {
		// TODO: this will not be needed when cards works as links with href [?]
		history.pushState(null, '', 'section/content/workspace/document/edit/' + id);
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
		if (this._loading) {
			return html`<div class="container"><uui-loader></uui-loader></div>`;
		}

		if (this._items.length === 0) {
			return html`<div class="container"><p>${this.localize.term('content_listViewNoItems')}</p></div>`;
		}

		return html`
			<div id="document-grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item, index) => this.#renderCard(index, item),
				)}
			</div>
		`;
	}

	#renderCard(index: number, item: UmbDocumentCollectionItemModel) {
		const sortOrder = this._skip + index;
		return html`
			<uui-card-content-node
				.name=${item.name ?? 'Unnamed Document'}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#isSelected(item)}
				@open=${() => this.#onOpen(item.unique ?? '')}
				@selected=${() => this.#onSelect(item)}
				@deselected=${() => this.#onDeselect(item)}>
				<umb-icon slot="icon" name=${item.icon}></umb-icon>
				${this.#renderState(item)} ${this.#renderProperties(sortOrder, item)}
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
				// TODO: [LK] Check if we have a `SplitPascalCase`-esque utility function that could be used here.
				return html`<uui-tag slot="tag" color="danger" look="secondary"
					>${item.state.replace(/([A-Z])/g, ' $1')}</uui-tag
				>`;
		}
	}

	#renderProperties(sortOrder: number, item: UmbDocumentCollectionItemModel) {
		if (!this._userDefinedProperties) return;
		return html`
			<ul>
				${repeat(
					this._userDefinedProperties,
					(column) => column.alias,
					(column) =>
						html`<li><span>${column.header}:</span> ${getPropertyValueByAlias(sortOrder, item, column.alias)}</li>`,
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
