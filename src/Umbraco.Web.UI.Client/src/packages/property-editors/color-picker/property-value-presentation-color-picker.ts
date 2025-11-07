import { customElement, html, nothing } from "@umbraco-cms/backoffice/external/lit";
import { UmbPropertyValuePresentationBaseElement, UmbPropertyValuePresentationDisplayOption } from "../../core/property-value-presentation/index.js";

@customElement("umb-color-picker-property-value-presentation")
export class UmbColorPickerPropertyValuePresentation extends UmbPropertyValuePresentationBaseElement {

  override render() {
    const color = this.#getColor();
    const label = this.#getLabel();
    const size = this.display == UmbPropertyValuePresentationDisplayOption.COLLECTION_CARD ? 10 : 12;
    return color
      ? html`<uui-color-swatch label="${label}" value="${color}" style="--uui-swatch-size: ${size}px" />`
      : nothing;
  }

  #getColor() {
    if (!this.value) {
      return null;
    }

    if (typeof this.value === 'string') {
      return this.value;
    }

    return this.value.value;
  }

  #getLabel() {
    if (!this.value) {
      return '';
    }

    if (typeof this.value === 'string') {
      return '';
    }

    return this.value.label;
  }
}

export default UmbColorPickerPropertyValuePresentation;

declare global {
  interface HTMLElementTagNameMap {
    ["umb-color-picker-property-value-presentation"]: UmbColorPickerPropertyValuePresentation;
  }
}
