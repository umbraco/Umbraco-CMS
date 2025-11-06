import { customElement, html, nothing } from "@umbraco-cms/backoffice/external/lit";
import { UmbPropertyValuePresentationBaseElement } from "../../../core/property-value-presentation/index.js";

@customElement("umb-date-time-with-time-zone-picker-property-value-presentation")
export class UmbDateTimePickerWithTimeZonePropertyValuePresentation extends UmbPropertyValuePresentationBaseElement {

  override render() {
    const date = this.#getDateTime();
    return date
      ? html`<span>${date}</span>`
      : nothing;
  }

  #getDateTime() {
    if (!this.value) {
      return null;
    }

    let result = new Date(this.value.date).toLocaleString();
    if (this.value.timeZone && this.value.timeZone.length > 0) {
      result += ' (' + this.value.timeZone + ')';
    }

    return result;
  }
}

export default UmbDateTimePickerWithTimeZonePropertyValuePresentation;

declare global {
  interface HTMLElementTagNameMap {
    ["umb-date-time-with-time-zone-picker-property-value-presentation"]: UmbDateTimePickerWithTimeZonePropertyValuePresentation;
  }
}
