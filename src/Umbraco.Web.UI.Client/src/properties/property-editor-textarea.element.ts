import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-property-editor-textarea')
class UmbPropertyEditorTextarea extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      uui-textarea {
        width: 100%;
      }
    `,
  ];

  @property()
  value = '';

  render() {
    return html`<uui-textarea .value=${this.value}></uui-textarea>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-property-editor-textarea': UmbPropertyEditorTextarea;
  }
}
