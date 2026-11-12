import type { ExampleProductColorItemModel } from '../../../../product-color/repository/item/types.js';
import { css, customElement, html, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbFilterActiveValueElementBase } from '@umbraco-cms/backoffice/filter';

@customElement('example-product-color-filter-active-value')
export class ExampleProductColorFilterActiveValueElement extends UmbFilterActiveValueElementBase<
	string,
	ExampleProductColorItemModel
> {
	override render() {
		const name = this._resolvedValue?.name ?? this.api?.value ?? '';
		return html`<umb-filter-active-value-layout .label=${name} @remove=${this._remove}>
			<span id="swatch" style=${styleMap({ backgroundColor: this._resolvedValue?.hex })}></span>
			${name}
		</umb-filter-active-value-layout>`;
	}

	static override styles = [
		css`
			#swatch {
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

export { ExampleProductColorFilterActiveValueElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-product-color-filter-active-value': ExampleProductColorFilterActiveValueElement;
	}
}
