import { UUISliderEvent } from '@umbraco-ui/uui';
import { css, CSSResultGroup, html, LitElement, PropertyValueMap } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { unsafeHTML } from 'lit/directives/unsafe-html.js';
import { UmbContextConsumerMixin } from '../core/context';
import { PostInstallRequest, TelemetryModel } from '../core/models';
import { UmbInstallerContext } from './installer-context';

@customElement('umb-installer-consent')
export class UmbInstallerConsent extends UmbContextConsumerMixin(LitElement) {
  static styles: CSSResultGroup = [
    css`
      h1 {
        text-align: center;
        margin-bottom: var(--uui-size-layout-3);
      }

      #buttons {
        display: flex;
        margin-top: var(--uui-size-layout-3);
      }

      #button-install {
        margin-left: auto;
        min-width: 120px;
      }
    `,
  ];

  @property({ attribute: false })
  private _telemetryLevels: TelemetryModel[] = [];

  @property({ attribute: false })
  public _telemetryFormData!: TelemetryModel['level'];

  @state()
  private _installerStore!: UmbInstallerContext;

  constructor() {
    super();

    this.consumeContext('umbInstallerContext', (installerStore: UmbInstallerContext) => {
      this._installerStore = installerStore;
      this._telemetryFormData = installerStore.getData().telemetryLevel;
      this._telemetryLevels = installerStore.getInstallerSettings().user.consentLevels;
    });
  }

  private _handleChange(e: InputEvent) {
    const target = e.target as HTMLInputElement;

    const value: { [key: string]: string } = {};
    value[target.name] = this._telemetryLevels[parseInt(target.value) - 1].level;
    this._installerStore.appendData(value);
  }

  private _onNext() {
    this.dispatchEvent(new CustomEvent('next', { bubbles: true, composed: true }));
  }

  private _onBack() {
    this.dispatchEvent(new CustomEvent('previous', { bubbles: true, composed: true }));
  }

  get _selectedTelemetryIndex() {
    return this._telemetryLevels?.findIndex((x) => x.level === this._telemetryFormData) ?? 0;
  }

  get _selectedTelemetry() {
    return this._telemetryLevels?.find((x) => x.level === this._telemetryFormData) ?? this._telemetryLevels[0];
  }

  private _renderSlider() {
    if (!this._telemetryLevels) return;

    return html`
      <uui-slider
        @input=${this._handleChange}
        name="telemetryLevel"
        label="telemetry-level"
        value=${this._selectedTelemetryIndex + 1}
        hide-step-values
        min="1"
        max=${this._telemetryLevels.length}></uui-slider>
      <h2>${this._selectedTelemetry.level}</h2>
      <!-- TODO: Is this safe to do? -->
      <p>${unsafeHTML(this._selectedTelemetry.description)}</p>
    `;
  }

  render() {
    return html`
      <div class="uui-text">
        <h1>Consent Level</h1>
        ${this._renderSlider()}
        <div id="buttons">
          <uui-button label="Back" @click=${this._onBack} look="secondary"></uui-button>
          <uui-button id="button-install" @click=${this._onNext} label="Next" look="primary"></uui-button>
        </div>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer-consent': UmbInstallerConsent;
  }
}
