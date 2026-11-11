import type { ExampleColorItemModel } from './types.js';
import { css, customElement, html, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbFacetFilterActiveValueElementBase } from '@umbraco-cms/backoffice/facet-filter';

@customElement('example-color-facet-filter-active-value')
export class ExampleColorFacetFilterActiveValueElement extends UmbFacetFilterActiveValueElementBase<
	string,
	ExampleColorItemModel
> {
	override render() {
		const name = this._resolvedValue?.name ?? this.api?.value ?? '';
		return html`<umb-facet-filter-active-value-layout .label=${name} @remove=${this._remove}>
			<span id="swatch" style=${styleMap({ backgroundColor: this._resolvedValue?.hex })}></span>
			${name}
		</umb-facet-filter-active-value-layout>`;
	}

	static override styles = [
		css`
			#swatch {
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

export { ExampleColorFacetFilterActiveValueElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-color-facet-filter-active-value': ExampleColorFacetFilterActiveValueElement;
	}
}
