import { customElement, html, nothing, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValuePresentationElementBase } from '@umbraco-cms/backoffice/property-value-presentation';

@customElement('umb-color-picker-property-value-presentation')
export class UmbColorPickerPropertyValuePresentationElement extends UmbPropertyValuePresentationElementBase<
	{ value: string; label: string } | string
> {
	override render() {
		if (!this.value) return nothing;
		const color = typeof this.value === 'string' ? this.value : this.value.value;
		const label = typeof this.value === 'string' ? this.value : this.value.label;
		return when(
			color,
			() => html`<uui-color-swatch label=${label} value=${color} style="--uui-swatch-size: 1em;"></uui-color-swatch>`,
		);
	}
}

export { UmbColorPickerPropertyValuePresentationElement as element };

export default UmbColorPickerPropertyValuePresentationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-color-picker-property-value-presentation': UmbColorPickerPropertyValuePresentationElement;
	}
}
