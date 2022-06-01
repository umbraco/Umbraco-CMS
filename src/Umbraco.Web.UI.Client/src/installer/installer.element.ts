import './installer-database.element';
import './installer-consent.element';
import './installer-installing.element';
import './installer-layout.element';
import './installer-user.element';

import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UmbInstallerContext } from './installer-context';
import { UmbContextProviderMixin } from '../core/context';

@customElement('umb-installer')
export class UmbInstaller extends UmbContextProviderMixin(LitElement) {
  static styles: CSSResultGroup = [css``];

  @state()
  step = 1;

  constructor() {
    super();
    this.provideContext('umbInstallerContext', new UmbInstallerContext());
  }

  connectedCallback(): void {
    super.connectedCallback();
    this.addEventListener('next', () => this._handleNext());
    this.addEventListener('previous', () => this._goToPreviousStep());
  }

  private _handleNext() {
    this.step++;
  }

  private _goToPreviousStep() {
    this.step--;
  }

  private _renderSection() {
    switch (this.step) {
      case 2:
        return html`<umb-installer-consent></umb-installer-consent>`;
      case 3:
        return html`<umb-installer-database></umb-installer-database>`;
      case 4:
        return html`<umb-installer-installing></umb-installer-installing>`;

      default:
        return html`<umb-installer-user></umb-installer-user>`;
    }
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
