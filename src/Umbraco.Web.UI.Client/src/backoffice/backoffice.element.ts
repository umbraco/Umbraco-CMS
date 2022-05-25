import './backoffice-header.element';
import './backoffice-sidebar.element';
import './backoffice-content.element';

import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';

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

      #main {
        display: flex;
        flex: 1;
        overflow: hidden;
      }
    `,
  ];

  // TODO: I would think umb-backoffice-header would be outside the router outlet? so its always present.

  render() {
    return html`
      <umb-backoffice-header></umb-backoffice-header>
      <div id="main">
        <umb-backoffice-sidebar></umb-backoffice-sidebar>
        <umb-backoffice-content></umb-backoffice-content>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice': UmbBackoffice;
  }
}
