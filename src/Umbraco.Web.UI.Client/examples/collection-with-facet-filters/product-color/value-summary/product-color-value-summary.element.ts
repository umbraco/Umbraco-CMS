import { css, customElement, html, nothing, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { ExampleProductColorItemModel } from '../repository/item/types.js';

@customElement('example-product-color-value-summary')
export class ExampleProductColorValueSummaryElement extends UmbValueSummaryElementBase<
	ReadonlyArray<ExampleProductColorItemModel>
> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`${this._value.map(
			(item) =>
				html`<span class="color"
					><span class="swatch" style=${styleMap({ backgroundColor: item.hex })}></span>${item.name}</span
				>`,
		)}`;
	}

	static override styles = [
		css`
			.color {
				margin-right: var(--uui-size-space-3);
				white-space: nowrap;
			}
			.swatch {
				display: inline-block;
				width: 10px;
				height: 10px;
				margin-right: var(--uui-size-space-1);
				border-radius: 50%;
				border: 1px solid var(--uui-color-border);
				vertical-align: middle;
			}
		`,
	];
}

export { ExampleProductColorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-product-color-value-summary': ExampleProductColorValueSummaryElement;
	}
}
