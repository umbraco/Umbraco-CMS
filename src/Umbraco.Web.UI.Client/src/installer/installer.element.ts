import './installer-database.element';
import './installer-consent.element';
import './installer-installing.element';
import './installer-layout.element';
import './installer-user.element';

import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getInstall, postInstall } from '../core/api/fetcher';
import { PostInstallRequest, UmbracoInstaller, UmbracoPerformInstallRequest } from '../core/models';

@customElement('umb-installer')
export class UmbInstaller extends LitElement {
  static styles: CSSResultGroup = [css``];

  @state()
  step = 1;

  @state()
  user = {};

  @state()
  data?: UmbracoPerformInstallRequest;

  private _handleSubmit(e: CustomEvent) {
    this.data = { ...this.data, ...e.detail };
    this._goToNextStep();
    console.log('data', this.data);
  }

  @state()
  installerSettings?: UmbracoInstaller;

  private _renderSection() {
    switch (this.step) {
      case 2:
        return html`<umb-installer-consent
          .telemetryLevels=${this.installerSettings?.user.consentLevels}
          .data=${this.data}
          @submit=${this._handleSubmit}></umb-installer-consent>`;
      case 3:
        return html`<umb-installer-database
          .databases=${this.installerSettings?.databases}
          .data=${this.data}
          @submit=${this._handleSubmit}></umb-installer-database>`;
      case 4:
        return html`<umb-installer-installing></umb-installer-installing>`;

      default:
        return html`<umb-installer-user
          .userModel=${this.installerSettings?.user}
          .data=${this.data}
          @submit=${this._handleSubmit}></umb-installer-user>`;
    }
  }

  connectedCallback(): void {
    super.connectedCallback();
    this.addEventListener('next', () => this._goToNextStep());
    this.addEventListener('previous', () => this._goToPreviousStep());

    getInstall({}).then(({ data }) => {
      this.installerSettings = data;
      console.log('Install data response', data);
    });
  }

  private _goToNextStep() {
    //TODO: Fix with router
    if (this.step === 3) {
      if (this.data) {
        this._postInstall(this.data);
      }
    } else {
      this.step++;
    }
  }

  private _goToPreviousStep() {
    this.step--;
  }

  private async _postInstall(data: PostInstallRequest) {
    try {
      await postInstall(data);
      this.step = 3; //TODO: Fix with router
    } catch (error) {
      console.log(error);
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
