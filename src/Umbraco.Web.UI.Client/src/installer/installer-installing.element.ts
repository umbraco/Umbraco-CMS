import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
@customElement('umb-installer-installing')
export class UmbInstallerInstalling extends LitElement {
  static styles: CSSResultGroup = [
    css`
      h1 {
        text-align: center;
      }
    `,
  ];

  render() {
    return html` <div class="uui-text">
      <h1 class="uui-h3">Installing Umbraco</h1>
      <uui-progress-bar progress="50"></uui-progress-bar>
    </div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer-installing': UmbInstallerInstalling;
  }
}
