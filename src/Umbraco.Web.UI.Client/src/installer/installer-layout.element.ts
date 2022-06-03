import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-installer-layout')
export class UmbInstallerLayout extends LitElement {
  static styles: CSSResultGroup = [
    css`
      #background {
        position: fixed;
        overflow: hidden;
        background-position: 50%;
        background-repeat: no-repeat;
        background-size: cover;
        background-image: url('/installer.jpg');
        width: 100vw;
        height: 100vh;
      }

      #logo {
        position: fixed;
        top: var(--uui-size-space-5);
        left: var(--uui-size-space-5);
        height: 30px;
      }

      #logo img {
        height: 100%;
      }

      #container {
        position: relative;
        display: flex;
        justify-content: flex-end;
        width: 100vw;
        height: 100vh;
      }

      #box {
        max-width: 500px;
        width: 100%;
        box-sizing: border-box;
        border-radius: 30px 0 0 30px;
        background-color: var(--uui-color-surface);
        display: flex;
        flex-direction: column;
        padding: var(--uui-size-layout-4) var(--uui-size-layout-4) var(--uui-size-layout-2) var(--uui-size-layout-4);
      }
    `,
  ];

  render() {
    return html`<div>
      <div id="background"></div>

      <div id="logo">
        <img src="/umbraco_logo_white.svg" alt="Umbraco" />
      </div>

      <div id="container">
        <div id="box">
          <slot></slot>
        </div>
      </div>
    </div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer-layout': UmbInstallerLayout;
  }
}
