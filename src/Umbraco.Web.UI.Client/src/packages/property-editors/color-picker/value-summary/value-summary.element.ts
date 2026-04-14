import type { UmbColorPickerPropertyEditorValue } from '../value-type/types.js';
import { customElement, html, nothing, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-color-picker-property-editor-value-summary')
export class UmbColorPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<UmbColorPickerPropertyEditorValue> {
	override render() {
		if (!this._value) return nothing;
		const color = typeof this._value === 'string' ? this._value : this._value.value;
		const label = typeof this._value === 'string' ? this._value : this._value.label;
		return when(
			color,
			() => html`<uui-color-swatch label=${label} value=${color} style="--uui-swatch-size: 1em;"></uui-color-swatch>`,
		);
	}
}

export { UmbColorPickerPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-color-picker-property-editor-value-summary': UmbColorPickerPropertyEditorValueSummaryElement;
	}
}
