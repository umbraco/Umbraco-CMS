import { UUISliderEvent } from '@umbraco-ui/uui';
import { css, CSSResultGroup, html, LitElement, PropertyValueMap } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { unsafeHTML } from 'lit/directives/unsafe-html.js';
import { PostInstallRequest, TelemetryModel } from '../core/models';

@customElement('umb-installer-consent')
export class UmbInstallerConsent extends LitElement {
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
  private telemetryLevels?: TelemetryModel[];

  @property({ attribute: false })
  public data?: PostInstallRequest;

  @state()
  private _selectedTelemetryIndex = 0;

  private _handleSubmit = (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const isValid = form.checkValidity();
    if (!isValid) return;

    const formData = new FormData(form);

    const telemetry = formData.get('telemetryLevel')?.toString();
    if (!telemetry) return;

    const telemetryLevel = this.telemetryLevels?.[parseInt(telemetry) - 1].level;

    this.dispatchEvent(new CustomEvent('submit', { detail: { telemetryLevel } }));
  };

  private _onBack() {
    this.dispatchEvent(new CustomEvent('previous', { bubbles: true, composed: true }));
  }

  private _updateTelemetryIndex = (e: UUISliderEvent) => {
    this._selectedTelemetryIndex = parseInt(e.target.value.toString()) - 1;
  };

  private _renderSlider() {
    if (!this.telemetryLevels) return;

    const currentTelemetryLevel = this.telemetryLevels[this._selectedTelemetryIndex];

    return html`
      <uui-slider
        @input=${this._updateTelemetryIndex}
        name="telemetryLevel"
        label="telemetry-level"
        value=${this._selectedTelemetryIndex + 1}
        hide-step-values
        min="1"
        max=${this.telemetryLevels.length}></uui-slider>
      <h2>${currentTelemetryLevel.level}</h2>
      <!-- TODO: Is this safe to do? -->
      <p>${unsafeHTML(currentTelemetryLevel.description)}</p>
    `;
  }

  protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
    super.firstUpdated(_changedProperties);

    this._selectedTelemetryIndex = this.telemetryLevels?.findIndex((x) => x.level === this.data?.telemetryLevel) ?? 0;
    this._selectedTelemetryIndex = this._selectedTelemetryIndex === -1 ? 0 : this._selectedTelemetryIndex; // If not found, default to first
  }

  render() {
    return html`
      <div class="uui-text">
        <h1>Consent Level</h1>
        <uui-form>
          <form id="LoginForm" name="login" @submit="${this._handleSubmit}">
            ${this._renderSlider()}
            <div id="buttons">
              <uui-button label="Back" @click=${this._onBack} look="secondary"></uui-button>
              <uui-button id="button-install" type="submit" label="Next" look="primary"></uui-button>
            </div>
          </form>
        </uui-form>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer-consent': UmbInstallerConsent;
  }
}
