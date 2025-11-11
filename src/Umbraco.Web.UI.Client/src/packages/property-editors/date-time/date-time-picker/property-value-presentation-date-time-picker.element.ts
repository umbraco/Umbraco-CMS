import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValuePresentationBaseElement } from '@umbraco-cms/backoffice/property-value-presentation';

@customElement('umb-date-time-picker-property-value-presentation')
export class UmbDateTimePickerPropertyValuePresentationElement extends UmbPropertyValuePresentationBaseElement<{
	date: string;
}> {
	override render() {
		if (!this.value?.date) return nothing;
		const date = new Date(this.value.date).toLocaleString();
		return date ? html`<span>${date}</span>` : nothing;
	}
}

export { UmbDateTimePickerPropertyValuePresentationElement as element };

export default UmbDateTimePickerPropertyValuePresentationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-time-picker-property-value-presentation': UmbDateTimePickerPropertyValuePresentationElement;
	}
}
