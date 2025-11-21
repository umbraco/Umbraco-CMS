import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValuePresentationElementBase } from '@umbraco-cms/backoffice/property-value-presentation';

@customElement('umb-date-time-picker-property-value-presentation')
export class UmbDateTimePickerPropertyValuePresentationElement extends UmbPropertyValuePresentationElementBase<{
	date: string;
}> {
	override render() {
		if (!this.value?.date) return nothing;
		const date = new Date(this.value.date);
		if (isNaN(date.getTime())) return nothing;
		return html`<span>${date.toLocaleString()}</span>`;
	}
}

export { UmbDateTimePickerPropertyValuePresentationElement as element };

export default UmbDateTimePickerPropertyValuePresentationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-time-picker-property-value-presentation': UmbDateTimePickerPropertyValuePresentationElement;
	}
}
