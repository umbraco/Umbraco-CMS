import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-installer')
export class UmbInstaller extends LitElement {
  static styles: CSSResultGroup = [
    css``,
  ];

  render() {
    return html`<div>Installer</div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer': UmbInstaller;
  }
}
