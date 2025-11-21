import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValuePresentationElementBase } from '@umbraco-cms/backoffice/property-value-presentation';

@customElement('umb-date-only-picker-property-value-presentation')
export class UmbDateOnlyPickerPropertyValuePresentationElement extends UmbPropertyValuePresentationElementBase<{
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
