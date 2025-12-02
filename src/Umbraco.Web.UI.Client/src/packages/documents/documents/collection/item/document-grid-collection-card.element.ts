import { UmbDocumentCollectionItemDataResolver } from '../document-collection-item-data-resolver.js';
import type { UmbDocumentCollectionItemModel } from '../types.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase, stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UUICardContentNodeElement } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestPropertyValuePresentation } from '@umbraco-cms/backoffice/property-value-presentation';
import type { UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-document-grid-collection-card')
export class UmbDocumentGridCollectionCardElement extends UmbElementMixin(UUICardContentNodeElement) {
	#resolver = new UmbDocumentCollectionItemDataResolver(this);

	@state()
	private _state?: string;

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

		this.#resolver.observe(this.#resolver.name, (name) => (this.name = name || ''));
		this.#resolver.observe(this.#resolver.state, (state) => (this._state = state || ''));
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
		let value: string | number | null | undefined;
		let editorAlias: string | null | undefined;

		if (column.isSystem) {
			value = this.#resolver.getSystemValue(column.alias);
		} else {
			const prop = this.#resolver.getPropertyByAlias(column.alias);
			editorAlias = prop?.editorAlias;
			value = prop?.value;
		}

		return html`
			<li>
				<span>${this.localize.string(column.header)}:</span>
				${when(
					column.nameTemplate,
					() => html`<umb-ufm-render inline .markdown=${column.nameTemplate} .value=${{ value }}></umb-ufm-render>`,
					() =>
						when(
							editorAlias,
							(schemaAlias) => html`
								<umb-extension-slot
									type="propertyValuePresentation"
									.filter=${(m: ManifestPropertyValuePresentation) =>
										stringOrStringArrayContains(m.forPropertyEditorSchemaAlias, schemaAlias)}
									.props=${{ value }}>
									${value ?? nothing}
								</umb-extension-slot>
							`,
							() => (value ? html`${value}` : nothing),
						),
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
