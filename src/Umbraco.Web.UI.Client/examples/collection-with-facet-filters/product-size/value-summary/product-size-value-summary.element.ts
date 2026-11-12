import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

@customElement('example-product-size-value-summary')
export class ExampleProductSizeValueSummaryElement extends UmbValueSummaryElementBase<ReadonlyArray<UmbItemModel>> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<span>${this._value.map((item) => item.name).join(', ')}</span>`;
	}
}

export { ExampleProductSizeValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-product-size-value-summary': ExampleProductSizeValueSummaryElement;
	}
}
