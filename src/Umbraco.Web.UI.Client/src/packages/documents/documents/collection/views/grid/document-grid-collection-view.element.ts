import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../../document-collection.context-token.js';
import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../../types.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import { getPropertyValueByAlias } from '../index.js';
import { css, customElement, html, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbDefaultCollectionContext, UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';

import '@umbraco-cms/backoffice/ufm';

@customElement('umb-document-grid-collection-view')
export class UmbDocumentGridCollectionViewElement extends UmbLitElement {
	@state()
	private _workspacePathBuilder?: UmbModalRouteBuilder;

	@state()
	private _items: Array<UmbDocumentCollectionItemModel> = [];

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _userDefinedProperties?: Array<UmbCollectionColumnConfiguration>;

	#collectionContext?: UmbDefaultCollectionContext<UmbDocumentCollectionItemModel, UmbDocumentCollectionFilterModel>;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			collectionContext?.setupView(this);
			this.observe(
				collectionContext?.workspacePathBuilder,
				(builder) => {
					this._workspacePathBuilder = builder;
				},
				'observePath',
			);
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
			'_observeUserDefinedProperties',
		);

		this.observe(this.#collectionContext.items, (items) => (this._items = items), '_observeItems');

		this.observe(
			this.#collectionContext.selection.selection,
			(selection) => (this._selection = selection),
			'_observeSelection',
		);
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

	#getEditUrl(item: UmbDocumentCollectionItemModel) {
		return item.unique && this._workspacePathBuilder
			? this._workspacePathBuilder({ entityType: item.entityType }) +
					UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateLocal({
						unique: item.unique,
					})
			: '';
	}

	override render() {
		return html`
			<div id="document-grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</div>
		`;
	}

	#renderItem(item: UmbDocumentCollectionItemModel) {
		return html`
			<uui-card-content-node
				.name=${item.name ?? 'Unnamed Document'}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#isSelected(item)}
				href=${this.#getEditUrl(item)}
				@selected=${() => this.#onSelect(item)}
				@deselected=${() => this.#onDeselect(item)}>
				<umb-icon slot="icon" name=${item.icon}></umb-icon>
				${this.#renderState(item)} ${this.#renderProperties(item)}
			</uui-card-content-node>
		`;
	}

	#getStateTagConfig(state: string): { color: UUIInterfaceColor; label: string } {
		switch (state) {
			case 'Published':
				return { color: 'positive', label: this.localize.term('content_published') };
			case 'PublishedPendingChanges':
				return { color: 'warning', label: this.localize.term('content_publishedPendingChanges') };
			case 'Draft':
				return { color: 'default', label: this.localize.term('content_unpublished') };
			case 'NotCreated':
				return { color: 'danger', label: this.localize.term('content_notCreated') };
			default:
				return { color: 'danger', label: fromCamelCase(state) };
		}
	}

	#renderState(item: UmbDocumentCollectionItemModel) {
		const tagConfig = this.#getStateTagConfig(item.state);
		return html`<uui-tag slot="tag" color=${tagConfig.color} look="secondary">${tagConfig.label}</uui-tag>`;
	}

	#renderProperties(item: UmbDocumentCollectionItemModel) {
		if (!this._userDefinedProperties) return;
		return html`
			<ul>
				${repeat(
					this._userDefinedProperties,
					(column) => column.alias,
					(column) => this.#renderProperty(item, column),
				)}
			</ul>
		`;
	}

	#renderProperty(item: UmbDocumentCollectionItemModel, column: UmbCollectionColumnConfiguration) {
		const value = getPropertyValueByAlias(item, column.alias);
		return html`
			<li>
				<span>${this.localize.string(column.header)}:</span>
				${when(
					column.nameTemplate,
					() => html`<umb-ufm-render inline .markdown=${column.nameTemplate} .value=${{ value }}></umb-ufm-render>`,
					() => html`${value}`,
				)}
			</li>
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
