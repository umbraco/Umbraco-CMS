import './installer-database.element';
import './installer-installing.element';
import './installer-layout.element';
import './installer-user.element';

import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getInstall } from '../api/fetcher';

@customElement('umb-installer')
export class UmbInstaller extends LitElement {
  static styles: CSSResultGroup = [css``];

  @state()
  step = 2;

  @state()
  user = {};

  @state()
  database = {};

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
    this.addEventListener('database', () => this._handleDatabase());
    this.addEventListener('user', () => this._handleUser());

    getInstall({}).then(({ data }) => {
      console.log('install data', data);
    });
  }

  private _handleUser() {
    this.step = 1;
  }

  private _handleDatabase() {
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
