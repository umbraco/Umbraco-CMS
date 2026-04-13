import type { UmbValueSummaryApi } from '../extensions/value-summary-api.interface.js';
import type { UmbValueSummaryElementInterface } from '../extensions/value-summary-element.interface.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-boolean-value-summary')
export class UmbBooleanValueSummaryElement extends UmbLitElement implements UmbValueSummaryElementInterface {
	@property({ attribute: false })
	set api(api: UmbValueSummaryApi | undefined) {
		this.#api = api;
		if (api) {
			this.observe(api.value, (v) => (this._value = v as boolean), 'value');
		}
	}
	get api() {
		return this.#api;
	}

	#api?: UmbValueSummaryApi;

	@state()
	private _value?: boolean | null;

	override render() {
		return html`${this._value === true
			? html`<uui-icon name="icon-true"></uui-icon>`
			: html`<uui-icon name="icon-false"></uui-icon>`}`;
	}
}

export { UmbBooleanValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-boolean-value-summary': UmbBooleanValueSummaryElement;
	}
}
