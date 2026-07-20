import type { UmbSliderPropertyEditorUiValue } from '../types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-slider-property-editor-value-summary')
export class UmbSliderPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<UmbSliderPropertyEditorUiValue> {
	override render() {
		if (!this._value) return nothing;

		if (this._value.from !== this._value.to) {
			return html`<span>${this._value.from}-${this._value.to}</span>`;
		} else {
			return html`<span>${this._value.from}</span>`;
		}
	}
}

export { UmbSliderPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-slider-property-editor-value-summary': UmbSliderPropertyEditorValueSummaryElement;
	}
}
