import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../document-collection.context-token.js';
import type { UmbDocumentCollectionFilterModel } from '../types.js';
import { UmbDocumentItemDataResolver } from '../../item/document-item-data-resolver.js';
import type { UmbDocumentCollectionItemModel } from './types.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	repeat,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import type {
	UmbCollectionColumnConfiguration,
	UmbDefaultCollectionContext,
	UmbEntityCollectionItemElement,
} from '@umbraco-cms/backoffice/collection';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';

@customElement('umb-document-collection-item-card')
export class UmbDocumentCollectionItemCardElement extends UmbLitElement implements UmbEntityCollectionItemElement {
	#item?: UmbDocumentCollectionItemModel | undefined;
	#resolver = new UmbDocumentItemDataResolver(this);

	@property({ type: Object })
	public get item(): UmbDocumentCollectionItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbDocumentCollectionItemModel | undefined) {
		this.#item = value;
		this.#resolver.setData(value);
	}

	@property({ type: Boolean })
	selectable = false;

	@property({ type: Boolean })
	selected = false;

	@property({ type: Boolean })
	selectOnly = false;

	@property({ type: Boolean })
	disabled = false;

	@property({ type: String })
	href?: string;

	@state()
	private _name?: string;

	@state()
	private _state?: string;

	@state()
	private _createDate?: Date;

	@state()
	private _updateDate?: Date;

	@state()
	private _userDefinedProperties?: Array<UmbCollectionColumnConfiguration>;

	#collectionContext?: UmbDefaultCollectionContext<UmbDocumentCollectionItemModel, UmbDocumentCollectionFilterModel>;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			this.#observeCollectionContext();
		});

		this.#resolver.observe(this.#resolver.name, (name) => (this._name = name || ''));
		this.#resolver.observe(this.#resolver.state, (state) => (this._state = state || ''));
		this.#resolver.observe(this.#resolver.createDate, (createDate) => (this._createDate = createDate));
		this.#resolver.observe(this.#resolver.updateDate, (updateDate) => (this._updateDate = updateDate));
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
	}

	#onSelected(event: CustomEvent) {
		if (!this.item) return;
		event.stopPropagation();
		this.dispatchEvent(new UmbSelectedEvent(this.item.unique));
	}

	#onDeselected(event: CustomEvent) {
		if (!this.item) return;
		event.stopPropagation();
		this.dispatchEvent(new UmbDeselectedEvent(this.item.unique));
	}

	#getPropertyValueByAlias(alias: string) {
		if (!this.item) return '';

		switch (alias) {
			case 'contentTypeAlias':
				return this.item.documentType.alias;
			case 'createDate':
				return this._createDate?.toLocaleString();
			case 'creator':
			case 'owner':
				return this.item.creator;
			case 'name':
				return this._name;
			case 'state':
				return this._state ? fromCamelCase(this._state) : '';
			case 'published':
				return this._state !== DocumentVariantStateModel.DRAFT ? 'True' : 'False';
			case 'sortOrder':
				return this.item.sortOrder;
			case 'updateDate':
				return this._updateDate?.toLocaleString();
			case 'updater':
				return this.item.updater;
			default: {
				const culture = this.#resolver.getCulture();
				const prop = this.item.values.find((x) => x.alias === alias && (!x.culture || x.culture === culture));
				return prop?.value ?? '';
			}
		}
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
		if (!this.item) return nothing;
		return html`
			<uui-card-content-node
				.name=${this._name}
				href=${ifDefined(this.href)}
				?selectable=${this.selectable}
				?select-only=${this.selectOnly}
				?selected=${this.selected}
				?disabled=${this.disabled}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}>
				${this.#renderIcon()} ${this.#renderState()}
				<div id="properties">${this.#renderProperties()}</div>
				<slot name="actions" slot="actions"></slot>
			</uui-card-content-node>
		`;
	}

	#renderIcon() {
		if (!this.item) return nothing;
		return html`<umb-icon slot="icon" name=${this.item.documentType.icon}></umb-icon>`;
	}

	#renderState() {
		const tagConfig = this.#getStateTagConfig();
		if (!tagConfig) return nothing;
		return html`<uui-tag slot="tag" id="state" color=${tagConfig.color} look="secondary">${tagConfig.label}</uui-tag>`;
	}

	#renderProperties() {
		if (!this._userDefinedProperties) return;
		return html`
			<ul>
				${repeat(
					this._userDefinedProperties,
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
		css`
			uui-card-content-node {
				min-width: auto;
				width: 100%;
				min-height: 180px;
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

export { UmbDocumentCollectionItemCardElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-collection-item-card': UmbDocumentCollectionItemCardElement;
	}
}
