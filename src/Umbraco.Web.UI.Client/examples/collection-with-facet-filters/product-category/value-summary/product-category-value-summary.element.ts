import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

@customElement('example-product-category-value-summary')
export class ExampleProductCategoryValueSummaryElement extends UmbValueSummaryElementBase<UmbItemModel | undefined> {
	override render() {
		if (!this._value) return nothing;
		return html`<span>${this._value.name}</span>`;
	}
}

export { ExampleProductCategoryValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-product-category-value-summary': ExampleProductCategoryValueSummaryElement;
	}
}
