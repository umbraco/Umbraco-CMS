import type { ExampleProductColorItemModel } from '../repository/item/types.js';
import { css, customElement, html, nothing, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('example-product-color-value-summary')
export class ExampleProductColorValueSummaryElement extends UmbValueSummaryElementBase<
	ReadonlyArray<ExampleProductColorItemModel>
> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<div id="colors">
			${this._value.map(
				(item) =>
					html`<span class="color"
						><span class="swatch" style=${styleMap({ backgroundColor: item.hex })}></span>${item.name}</span
					>`,
			)}
		</div>`;
	}

	static override styles = [
		css`
			#colors {
				display: flex;
			}

			.color {
				margin-right: var(--uui-size-space-3);
				white-space: nowrap;
				display: flex;
				align-items: center;
			}
			.swatch {
				display: inline-block;
				width: 8px;
				height: 8px;
				margin-right: var(--uui-size-space-1);
				padding: 1px;
				border-radius: 50%;
				border: 1px solid var(--uui-color-border);
				background-clip: content-box;
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
