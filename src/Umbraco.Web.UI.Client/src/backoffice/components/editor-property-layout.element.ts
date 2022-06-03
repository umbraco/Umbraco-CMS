import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';

@customElement('umb-editor-property-layout')
class UmbEditorPropertyLayout extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      :host {
        display: grid;
        grid-template-columns: 200px 600px;
        gap: 32px;
      }
    `,
  ];

  render() {
    return html`
      <slot name="header" class="header">
      </slot>
      <slot name="editor" class="editor">
      </slot>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-editor-property-layout': UmbEditorPropertyLayout;
  }
}
