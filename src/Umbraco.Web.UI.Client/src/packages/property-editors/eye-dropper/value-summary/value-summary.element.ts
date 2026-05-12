import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-eye-dropper-value-summary')
export class UmbEyeDropperValueSummaryElement extends UmbValueSummaryElementBase<string> {
	override render() {
		if (!this._value) return nothing;
		return html`<uui-color-swatch
			label=${this._value}
			value=${this._value}
			style="--uui-swatch-size: 1em;"></uui-color-swatch>`;
	}
}

export { UmbEyeDropperValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-eye-dropper-value-summary': UmbEyeDropperValueSummaryElement;
	}
}
