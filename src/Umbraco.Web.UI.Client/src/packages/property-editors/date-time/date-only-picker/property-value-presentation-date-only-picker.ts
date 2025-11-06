import { customElement, html, nothing } from "@umbraco-cms/backoffice/external/lit";
import { UmbPropertyValuePresentationBase } from "../../../core/property-value-presentation/property-value-presentation-base.js";

@customElement("umb-date-only-picker-property-value-presentation")
export class UmbDateOnlyPickerPropertyValuePresentation extends UmbPropertyValuePresentationBase {

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

    return this.value.date.toLocaleString();
  }
}

export default UmbDateOnlyPickerPropertyValuePresentation;

declare global {
  interface HTMLElementTagNameMap {
    ["umb-date-only-picker-property-value-presentation"]: UmbDateOnlyPickerPropertyValuePresentation;
  }
}
