import { UmbPropertyValuePresentationBaseElement } from '../../../core/property-value-presentation/index.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-date-time-picker-property-value-presentation')
export class UmbDateTimePickerPropertyValuePresentation extends UmbPropertyValuePresentationBaseElement {
	override render() {
		const date = this.#getDateTime();
		return date ? html`<span>${date}</span>` : nothing;
	}

	#getDateTime() {
		if (!this.value) {
			return null;
		}

		return new Date(this.value.date).toLocaleString();
	}
}

export default UmbDateTimePickerPropertyValuePresentation;

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-time-picker-property-value-presentation': UmbDateTimePickerPropertyValuePresentation;
	}
}
