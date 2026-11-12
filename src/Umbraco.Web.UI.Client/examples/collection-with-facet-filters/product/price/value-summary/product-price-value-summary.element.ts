import type { ExampleProductPriceModel } from '../types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('example-product-price-value-summary')
export class ExampleProductPriceValueSummaryElement extends UmbValueSummaryElementBase<ExampleProductPriceModel> {
	override render() {
		if (!this._value) return nothing;
		return html`<span
			>${this.localize.number(this._value.amount, { style: 'currency', currency: this._value.currency })}</span
		>`;
	}
}

export { ExampleProductPriceValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-product-price-value-summary': ExampleProductPriceValueSummaryElement;
	}
}
