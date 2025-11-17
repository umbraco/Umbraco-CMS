import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValuePresentationBaseElement } from '@umbraco-cms/backoffice/property-value-presentation';

@customElement('umb-date-only-picker-property-value-presentation')
export class UmbDateOnlyPickerPropertyValuePresentationElement extends UmbPropertyValuePresentationBaseElement<{
	date?: string;
}> {
	override render() {
		if (!this.value || !this.value.date) return nothing;
		const date = new Date(this.value.date);
		if (isNaN(date.getTime())) return nothing;
		return html`<span>${date.toLocaleDateString()}</span>`;
	}
}

export { UmbDateOnlyPickerPropertyValuePresentationElement as element };

export default UmbDateOnlyPickerPropertyValuePresentationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-only-picker-property-value-presentation': UmbDateOnlyPickerPropertyValuePresentationElement;
	}
}
