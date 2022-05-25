import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-property-editor-text')
class UmbPropertyEditorText extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      uui-input {
        width: 100%;
      }
    `,
  ];

  @property()
  value = '';

  render() {
    return html`<uui-input .value=${this.value} type="text"></uui-input>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-property-editor-text': UmbPropertyEditorText;
  }
}
