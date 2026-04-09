import { UmbDocumentItemDataResolver } from '../../item/document-item-data-resolver.js';
import type { UmbDocumentItemModel } from '../../item/repository/types.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-start-node-value-summary')
export class UmbDocumentStartNodeValueSummaryElement extends UmbLitElement {
	@property({ attribute: false })
	set value(item: UmbDocumentItemModel | null | undefined) {
		this.#item = item;
		if (item) {
			this.#resolver.setData(item);
		}
	}

	@state()
	private _name?: string;

	#item?: UmbDocumentItemModel | null;
	#resolver = new UmbDocumentItemDataResolver(this);

	constructor() {
		super();
		this.observe(this.#resolver.name, (name) => (this._name = name));
	}

	override render() {
		if (!this.#item) {
			return html`<span>${this.localize.term('content_contentRoot')}</span>`;
		}
		return html`<span>${this._name}</span>`;
	}
}

export { UmbDocumentStartNodeValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-start-node-value-summary': UmbDocumentStartNodeValueSummaryElement;
	}
}
