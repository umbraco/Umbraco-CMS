import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValuePresentationElementBase } from '@umbraco-cms/backoffice/property-value-presentation';

@customElement('umb-date-time-with-time-zone-picker-property-value-presentation')
export class UmbDateTimePickerWithTimeZonePropertyValuePresentationElement extends UmbPropertyValuePresentationElementBase<{
	date: string;
	timeZone: string;
}> {
	override render() {
		if (!this.value) return nothing;
		return html`<span>${this.#renderDate()}${this.#renderTimeZone()}</span>`;
	}

	#renderDate() {
		if (!this.value?.date) return nothing;
		const date = new Date(this.value.date);
		if (isNaN(date.getTime())) return nothing;
		return html`${date.toLocaleString()}`;
	}

	#renderTimeZone() {
		if (!this.value?.timeZone) return nothing;
		return html` (${this.value.timeZone})`;
	}
}

export { UmbDateTimePickerWithTimeZonePropertyValuePresentationElement as element };

export default UmbDateTimePickerWithTimeZonePropertyValuePresentationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-time-with-time-zone-picker-property-value-presentation': UmbDateTimePickerWithTimeZonePropertyValuePresentationElement;
	}
}
