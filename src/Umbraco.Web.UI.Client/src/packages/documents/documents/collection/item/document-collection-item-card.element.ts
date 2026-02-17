import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../document-collection.context-token.js';
import type { UmbDocumentCollectionFilterModel } from '../types.js';
import type { UmbDocumentCollectionItemModel } from './types.js';
import { css, customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import type {
	UmbCollectionColumnConfiguration,
	UmbDefaultCollectionContext,
	UmbEntityCollectionItemElement,
} from '@umbraco-cms/backoffice/collection';

import './document-grid-collection-card.element.js';

@customElement('umb-document-collection-item-card')
export class UmbDocumentCollectionItemCardElement extends UmbLitElement implements UmbEntityCollectionItemElement {
	#item?: UmbDocumentCollectionItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbDocumentCollectionItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbDocumentCollectionItemModel | undefined) {
		this.#item = value;
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
	private _userDefinedProperties?: Array<UmbCollectionColumnConfiguration>;

	#collectionContext?: UmbDefaultCollectionContext<UmbDocumentCollectionItemModel, UmbDocumentCollectionFilterModel>;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_COLLECTION_CONTEXT, (collectionContext) => {
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

	override render() {
		if (!this.item) return nothing;
		return html`
			<umb-document-grid-collection-card
				href=${ifDefined(this.href)}
				.item=${this.item}
				.columns=${this._userDefinedProperties}
				?selectable=${this.selectable}
				?select-only=${this.selectOnly}
				?selected=${this.selected}
				?disabled=${this.disabled}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}>
				<umb-icon slot="icon" name=${this.item.documentType.icon}></umb-icon>
			</umb-document-grid-collection-card>
		`;
	}

	static override styles = [
		css`
			umb-document-grid-collection-card {
				min-width: auto;
				width: 100%;
				min-height: 180px;
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
