import type { UmbValueSummaryApi } from '../extensions/value-summary-api.interface.js';
import type { UmbValueSummaryElement } from '../extensions/value-summary-element.interface.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-value-summary')
export class UmbDefaultValueSummaryElement extends UmbLitElement implements UmbValueSummaryElement {
	@property({ attribute: false })
	set api(api: UmbValueSummaryApi | undefined) {
		this.#api = api;
		if (api) {
			this.observe(api.value, (v) => (this._value = v), 'value');
		}
	}
	get api() {
		return this.#api;
	}

	#api?: UmbValueSummaryApi;

	@state()
	private _value?: unknown;

	override render() {
		return html`<span>${String(this._value ?? '')}</span>`;
	}
}

export { UmbDefaultValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-value-summary': UmbDefaultValueSummaryElement;
	}
}
