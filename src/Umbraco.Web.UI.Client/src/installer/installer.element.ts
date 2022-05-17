import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import './installer-layout.element';
import './installer-user.element';

@customElement('umb-installer')
export class UmbInstaller extends LitElement {
  static styles: CSSResultGroup = [css``];

  @state()
  step = 1;

  private _renderSection() {
    switch (this.step) {
      case 2:
        return html`<div>database</div>`;
      case 3:
        return html`<div>installing</div>`;

      default:
        return html`<umb-installer-user></umb-installer-user>`;
    }
  }

  connectedCallback(): void {
    super.connectedCallback();
    this.addEventListener('install', () => this._handleInstall());
  }

  private _handleInstall() {
    this.step++;
  }

  render() {
    return html`<umb-installer-layout
      >${this._renderSection()}

      <uui-button @click=${() => this.step--}>Back</uui-button>
      <uui-button @click=${() => this.step++}>Next</uui-button>
    </umb-installer-layout> `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer': UmbInstaller;
  }
}
