import type { ExampleColorItemModel } from './types.js';
import { css, customElement, html, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbFilterActiveValueElementBase } from '@umbraco-cms/backoffice/filter';

@customElement('example-color-filter-active-value')
export class ExampleColorFilterActiveValueElement extends UmbFilterActiveValueElementBase<
	string,
	ExampleColorItemModel
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

export { ExampleColorFilterActiveValueElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-color-filter-active-value': ExampleColorFilterActiveValueElement;
	}
}
