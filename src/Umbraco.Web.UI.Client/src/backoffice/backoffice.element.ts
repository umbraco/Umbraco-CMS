import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';

import './backoffice-header.element';
import './backoffice-sidebar.element';
import './backoffice-main.element';

@defineElement('umb-backoffice')
export class UmbBackoffice extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      :host {
        display: flex;
        flex-direction: column;
        height: 100%;
        width: 100%;
        color: var(--uui-color-text);
        font-size: 14px;
        box-sizing: border-box;
      }
    `,
  ];

  render() {
    return html`
      <umb-backoffice-header></umb-backoffice-header>
      <umb-backoffice-main></umb-backoffice-main>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice': UmbBackoffice;
  }
}
