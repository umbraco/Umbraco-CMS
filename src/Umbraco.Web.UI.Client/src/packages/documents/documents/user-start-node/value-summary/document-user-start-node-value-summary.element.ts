import { UmbDocumentItemDataResolver } from '../../item/document-item-data-resolver.js';
import type { UmbDocumentItemModel } from '../../item/repository/types.js';
import { customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-document-user-start-node-value-summary')
export class UmbDocumentUserStartNodeValueSummaryElement extends UmbValueSummaryElementBase<UmbDocumentItemModel | null> {
	@state()
	private _name?: string;

	#resolver = new UmbDocumentItemDataResolver(this);

	constructor() {
		super();
		this.observe(this.#resolver.name, (name) => (this._name = name));
	}

	protected override willUpdate(changedProperties: PropertyValueMap<this>): void {
		super.willUpdate(changedProperties);
		if (changedProperties.has('_value' as keyof this) && this._value) {
			this.#resolver.setData(this._value);
		}
	}

	override render() {
		if (!this._value) {
			return html`<span>${this.localize.term('content_contentRoot')}</span>`;
		}
		return html`<span>${this._name}</span>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-user-start-node-value-summary': UmbDocumentUserStartNodeValueSummaryElement;
	}
}
