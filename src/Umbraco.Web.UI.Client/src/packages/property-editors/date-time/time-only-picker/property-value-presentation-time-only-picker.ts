import { UmbPropertyValuePresentationBaseElement } from '../../../core/property-value-presentation/index.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-time-only-picker-property-value-presentation')
export class UmbTimeOnlyPickerPropertyValuePresentation extends UmbPropertyValuePresentationBaseElement {
	override render() {
		const time = this.#getTime();
		return time ? html`<span>${time}</span>` : nothing;
	}

	#getTime() {
		if (!this.value) {
			return null;
		}

		return this.value.date;
	}
}

export default UmbTimeOnlyPickerPropertyValuePresentation;

declare global {
	interface HTMLElementTagNameMap {
		'umb-time-only-picker-property-value-presentation': UmbTimeOnlyPickerPropertyValuePresentation;
	}
}
