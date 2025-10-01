import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../../document-collection.context-token.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../../types.js';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbDefaultCollectionContext, UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '@umbraco-cms/backoffice/ufm';
import './document-grid-collection-card.element.js';

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
			<umb-document-grid-collection-card
				class="document-grid-item"
				href=${this.#getEditUrl(item)}
				.item=${item}
				.columns=${this._userDefinedProperties}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#isSelected(item)}
				@selected=${() => this.#onSelect(item)}
				@deselected=${() => this.#onDeselect(item)}>
				<umb-icon slot="icon" name=${item.documentType.icon}></umb-icon>
			</umb-document-grid-collection-card>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			#document-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
				gap: var(--uui-size-space-4);
			}

			.document-grid-item {
				width: 100%;
				min-height: 180px;
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
