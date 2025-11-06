import { customElement, html, nothing } from "@umbraco-cms/backoffice/external/lit";
import { UmbPropertyValuePresentationBaseElement } from "../../../core/property-value-presentation/index.js";

@customElement("umb-date-only-picker-property-value-presentation")
export class UmbDateOnlyPickerPropertyValuePresentation extends UmbPropertyValuePresentationBaseElement {

  override render() {
    const date = this.#getDate();
    return date
      ? html`<span>${date}</span>`
      : nothing;
  }

  #getDate() {
    if (!this.value) {
      return null;
    }

    return new Date(this.value.date).toLocaleDateString();
  }
}

export default UmbDateOnlyPickerPropertyValuePresentation;

declare global {
  interface HTMLElementTagNameMap {
    ["umb-date-only-picker-property-value-presentation"]: UmbDateOnlyPickerPropertyValuePresentation;
  }
}
