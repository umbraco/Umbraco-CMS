import {LitElement, html, css} from "lit";
import { customElement, property } from "lit/decorators.js";
import { UmbPropertyEditorExtensionElement } from "@umbraco-cms/backoffice/extension-registry";

import type {MyCardElement} from "./my-card.element.ts";
import './my-card.element.js';

@customElement("my-property-editor-ui")
export class MyPropertyEditorUiElement
  extends LitElement
  implements UmbPropertyEditorExtensionElement
{
  @property({ type: String })
  public value = "";

  #onCardSelected(evt: Event) {
    const card = evt.target as MyCardElement;
    this.value = card.value ?? "";
    this.dispatchEvent(new CustomEvent("property-value-change"));
  }

  render() {
    return html`
      <div class="cards">
        <my-card value="VISA" ?selected=${this.value === 'VISA'} @selected=${this.#onCardSelected}>VISA</my-card>
        <my-card value="MasterCard" ?selected=${this.value === 'MasterCard'} @selected=${this.#onCardSelected}>MasterCard</my-card>
      </div>
    `;
  }

  static styles = [
    css`
      :host {
        display: block;
      }

      .cards {
        display: flex;
        flex-direction: row;
        gap: 1rem;
      }
    `
  ]
}

export default MyPropertyEditorUiElement;

declare global {
  interface HTMLElementTagNameMap {
    "my-property-editor-ui": MyPropertyEditorUiElement;
  }
}