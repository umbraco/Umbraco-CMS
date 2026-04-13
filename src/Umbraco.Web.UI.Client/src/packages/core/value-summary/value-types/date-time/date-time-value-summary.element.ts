import type { UmbValueSummaryApi } from '../../extensions/value-summary-api.interface.js';
import type { UmbValueSummaryElement } from '../../extensions/value-summary-element.interface.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-date-time-value-summary')
export class UmbDateTimeValueSummaryElement extends UmbLitElement implements UmbValueSummaryElement {
	@property({ attribute: false })
	set api(api: UmbValueSummaryApi | undefined) {
		this.#api = api;
		if (api) {
			this.observe(api.value, (v) => (this._value = v as string | null), 'value');
		}
	}
	get api() {
		return this.#api;
	}

	#api?: UmbValueSummaryApi;

	@state()
	private _value?: string | null;

	override render() {
		if (!this._value) return html`<span>—</span>`;
		return html`<span>${this.localize.dateTime(this._value)}</span>`;
	}
}

export { UmbDateTimeValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-time-value-summary': UmbDateTimeValueSummaryElement;
	}
}
