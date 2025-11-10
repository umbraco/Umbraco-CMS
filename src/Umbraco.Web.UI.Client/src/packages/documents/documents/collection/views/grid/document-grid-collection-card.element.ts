import { UmbDocumentItemDataResolver } from '../../../item/document-item-data-resolver.js';
import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../../document-collection.context-token.js';
import type { UmbDocumentCollectionContext } from '../../document-collection.context.js';
import type { UmbDocumentCollectionItemModel } from '../../types.js';
import { css, customElement, html, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UUICardContentNodeElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-document-grid-collection-card')
export class UmbDocumentGridCollectionCardElement extends UmbElementMixin(UUICardContentNodeElement) {
	#collectionContext?: UmbDocumentCollectionContext;

	#resolver = new UmbDocumentItemDataResolver(this);

	@state()
	private _createDate?: Date;

	@state()
	private _state?: string;

	@state()
	private _updateDate?: Date;

	@property({ attribute: false })
	columns?: Array<UmbCollectionColumnConfiguration>;

	@property({ attribute: false })
	public set item(value: UmbDocumentCollectionItemModel) {
		this.#item = value;
		if (value) {
			this.#resolver.setData(value);
		}
	}
	public get item(): UmbDocumentCollectionItemModel {
		return this.#item;
	}
	#item!: UmbDocumentCollectionItemModel;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});

		this.#resolver.observe(this.#resolver.name, (name) => (this.name = name || ''));
		this.#resolver.observe(this.#resolver.state, (state) => (this._state = state || ''));
		this.#resolver.observe(this.#resolver.createDate, (createDate) => (this._createDate = createDate));
		this.#resolver.observe(this.#resolver.updateDate, (updateDate) => (this._updateDate = updateDate));
	}

	#getPropertyValueByAlias(alias: string) {
		const args = {
			alias: alias,
			documentTypeAlias: this.item.documentType.alias,
			createDate: this._createDate,
			updateDate: this._updateDate,
			state: this._state,
			culture: this.#resolver.getCulture(),
			creator: this.item.creator,
			updater: this.item.updater,
			sortOrder: this.item.sortOrder,
			values: this.item.values,
		};
		return this.#collectionContext?.getPropertyValueByAlias(args);
	}

	#getStateTagConfig(): { color: UUIInterfaceColor; label: string } | undefined {
		if (!this._state) return;
		switch (this._state) {
			case DocumentVariantStateModel.PUBLISHED:
				return { color: 'positive', label: this.localize.term('content_published') };
			case DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES:
				return { color: 'warning', label: this.localize.term('content_publishedPendingChanges') };
			case DocumentVariantStateModel.DRAFT:
				return { color: 'default', label: this.localize.term('content_unpublished') };
			case DocumentVariantStateModel.NOT_CREATED:
				return { color: 'danger', label: this.localize.term('content_notCreated') };
			default:
				return { color: 'danger', label: fromCamelCase(this._state) };
		}
	}

	override render() {
		return html`${super.render()} ${this.#renderState()}`;
	}

	protected override renderDetail() {
		return html`
			${super.renderDetail()}
			<div id="properties">${this.#renderProperties()}</div>
		`;
	}

	#renderState() {
		const tagConfig = this.#getStateTagConfig();
		if (!tagConfig) return;
		return html`<uui-tag id="state" color=${tagConfig.color} look="secondary">${tagConfig.label}</uui-tag>`;
	}

	#renderProperties() {
		if (!this.columns) return;
		return html`
			<ul>
				${repeat(
					this.columns,
					(column) => column.alias,
					(column) => this.#renderProperty(column),
				)}
			</ul>
		`;
	}

	#renderProperty(column: UmbCollectionColumnConfiguration) {
		const value = this.#getPropertyValueByAlias(column.alias);
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
		...UUICardContentNodeElement.styles,
		css`
			#state {
				position: absolute;
				top: var(--uui-size-4);
				right: var(--uui-size-4);
				display: flex;
				justify-content: right;
			}

			#properties {
				font-size: var(--uui-type-small-size);
				line-height: calc(2 * var(--uui-size-3));

				> ul {
					list-style: none;
					padding-inline-start: 0px;
					margin: 0;

					> li > span {
						font-weight: 700;
					}
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-grid-collection-card': UmbDocumentGridCollectionCardElement;
	}
}
