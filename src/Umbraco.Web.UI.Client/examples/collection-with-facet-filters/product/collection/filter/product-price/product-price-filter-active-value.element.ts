import { EXAMPLE_PRODUCT_PRICE_CURRENCY } from './constants.js';
import type { ExampleProductPriceFilterValueModel } from './types.js';
import { UmbFilterActiveValueElementBase } from '@umbraco-cms/backoffice/filter';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('example-product-price-filter-active-value')
export class ExampleProductPriceFilterActiveValueElement extends UmbFilterActiveValueElementBase<ExampleProductPriceFilterValueModel> {
	override render() {
		const value = this.api?.value;
		if (!value) return;

		const amount = this.localize.number(value.amount, { style: 'currency', currency: EXAMPLE_PRODUCT_PRICE_CURRENCY });
		const key =
			value.key === 'min' ? '#exampleProductPriceFilter_minValueLabel' : '#exampleProductPriceFilter_maxValueLabel';
		const label = this.localize.string(key, amount);

		return html`<umb-filter-active-value-layout .label=${label} @remove=${this._remove}>
			${label}
		</umb-filter-active-value-layout>`;
	}
}

export { ExampleProductPriceFilterActiveValueElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-product-price-filter-active-value': ExampleProductPriceFilterActiveValueElement;
	}
}
