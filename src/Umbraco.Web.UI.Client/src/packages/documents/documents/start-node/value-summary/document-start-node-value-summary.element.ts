import { UmbDocumentItemDataResolver } from '../../item/document-item-data-resolver.js';
import type { UmbDocumentItemModel } from '../../item/repository/types.js';
import type { UmbValueSummaryApi, UmbValueSummaryElement } from '@umbraco-cms/backoffice/value-summary';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-start-node-value-summary')
export class UmbDocumentStartNodeValueSummaryElement extends UmbLitElement implements UmbValueSummaryElement {
	@property({ attribute: false })
	set api(api: UmbValueSummaryApi | undefined) {
		this.#api = api;
		if (api) {
			this.observe(
				api.value,
				(v) => {
					this.#item = v as UmbDocumentItemModel | null;
					if (this.#item) {
						this.#resolver.setData(this.#item);
					}
				},
				'value',
			);
		}
	}

	get api() {
		return this.#api;
	}

	@state()
	private _name?: string;

	#api?: UmbValueSummaryApi;
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
