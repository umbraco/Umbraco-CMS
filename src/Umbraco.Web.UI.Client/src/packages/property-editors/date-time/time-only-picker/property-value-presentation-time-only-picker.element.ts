import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValuePresentationElementBase } from '@umbraco-cms/backoffice/property-value-presentation';

@customElement('umb-time-only-picker-property-value-presentation')
export class UmbTimeOnlyPickerPropertyValuePresentationElement extends UmbPropertyValuePresentationElementBase<{
	date: string;
}> {
	override render() {
		if (!this.value?.date) return nothing;
		return html`<span>${this.value.date}</span>`;
	}
}

export { UmbTimeOnlyPickerPropertyValuePresentationElement as element };

export default UmbTimeOnlyPickerPropertyValuePresentationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-time-only-picker-property-value-presentation': UmbTimeOnlyPickerPropertyValuePresentationElement;
	}
}
