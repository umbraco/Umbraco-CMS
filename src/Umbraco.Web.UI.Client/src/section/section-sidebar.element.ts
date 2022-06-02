import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-sidebar')
export class UmbSectionSidebar extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      :host {
        flex: 0 0 300px;
        background-color: var(--uui-color-surface);
        height: 100%;
        border-right: 1px solid var(--uui-color-border);
        font-weight: 500;
        display: flex;
        flex-direction: column;
      }
    `,
  ];

  render() {
    return html`<slot></slot>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-section-sidebar': UmbSectionSidebar;
  }
}
