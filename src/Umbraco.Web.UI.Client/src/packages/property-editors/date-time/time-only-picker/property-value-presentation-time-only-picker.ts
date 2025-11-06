import { customElement, html, nothing } from "@umbraco-cms/backoffice/external/lit";
import { UmbPropertyValuePresentationBaseElement } from "../../../core/property-value-presentation/index.js";

@customElement("umb-time-only-picker-property-value-presentation")
export class UmbTimeOnlyPickerPropertyValuePresentation extends UmbPropertyValuePresentationBaseElement {

  override render() {
    const time = this.#getTime();
    return time
      ? html`<span>${time}</span>`
      : nothing;
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
    ["umb-time-only-picker-property-value-presentation"]: UmbTimeOnlyPickerPropertyValuePresentation;
  }
}
