// make new lit element that extends UUIInputElement

import { UUIInputElement } from '@umbraco-ui/uui';
import { customElement } from 'lit/decorators.js';

@customElement('umb-login-input')
export class UmbLoginInputElement extends UUIInputElement {
  protected createRenderRoot() {
    return this;
  }
  static styles = [...UUIInputElement.styles];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-login-input': UmbLoginInputElement;
  }
}
