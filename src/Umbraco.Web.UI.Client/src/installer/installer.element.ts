import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import './installer-layout.element';
import './installer-user.element';
import './installer-database.element';
import './installer-installing.element';

@customElement('umb-installer')
export class UmbInstaller extends LitElement {
  static styles: CSSResultGroup = [css``];

  @state()
  step = 1;

  private _renderSection() {
    switch (this.step) {
      case 2:
        return html`<umb-installer-database></umb-installer-database>`;
      case 3:
        return html`<umb-installer-installing></umb-installer-installing>`;

      default:
        return html`<umb-installer-user></umb-installer-user>`;
    }
  }

  connectedCallback(): void {
    super.connectedCallback();
    this.addEventListener('install', () => this._handleInstall());
    this.addEventListener('customize', () => this._handleCustomize());
    this.addEventListener('user', () => this._handleUser());
  }

  private _handleUser() {
    this.step = 1;
  }

  private _handleCustomize() {
    this.step = 2;
  }

  private _handleInstall() {
    this.step = 3;
  }

  render() {
    return html`<umb-installer-layout>${this._renderSection()}</umb-installer-layout> `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer': UmbInstaller;
  }
}
