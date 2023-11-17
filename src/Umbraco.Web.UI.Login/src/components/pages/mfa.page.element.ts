import type {UUIButtonState, UUIInputElement} from '@umbraco-ui/uui';
import {LitElement, css, html, nothing} from 'lit';
import {customElement, state} from 'lit/decorators.js';
import {until} from 'lit/directives/until.js';
import {umbAuthContext} from '../../context/auth.context.js';
import {umbLocalizationContext} from '../../external/localization/localization-context.js';
import {loadCustomView, renderCustomView} from '../../utils/load-custom-view.function.js';

type MfaCustomViewElement = HTMLElement & {
  providers?: string[];
  returnPath?: string;
};

@customElement('umb-mfa-page')
export default class UmbMfaPageElement extends LitElement {
  @state()
  protected providers: Array<{ name: string; value: string; selected: boolean }> = [];

  @state()
  private loading = true;

  @state()
  private buttonState?: UUIButtonState;

  @state()
  private error: string | null = null;

  constructor() {
    super();
    this.#loadProviders();
  }

  async #loadProviders() {
    try {
      const response = await umbAuthContext.getMfaProviders();
      this.providers = response.providers.map((provider) => ({name: provider, value: provider, selected: false}));

      if (this.providers.length) {
        this.providers[0].selected = true;
      }

      if (response.error) {
        this.error = response.error;
      }
    } catch (e) {
      if (e instanceof Error) {
        this.error = e.message ?? 'Unknown error';
      } else {
        this.error = 'Unknown error';
      }
      this.providers = [];
    }
    this.loading = false;
  }

  private async handleSubmit(e: SubmitEvent) {
    e.preventDefault();

    this.error = null;

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const codeInput = form.elements.namedItem('2facode') as UUIInputElement;

    if (codeInput) {
      codeInput.error = false;
      codeInput.errorMessage = '';
    }

    if (!form.checkValidity()) return;

    const formData = new FormData(form);

    let provider = formData.get('provider') as string;

    // If no provider given, use the first one (there probably is only one anyway)
    if (!provider) {
      provider = this.providers[0].value;
    }

    if (!provider) {
      this.error = 'No provider selected';
      return;
    }

    const code = formData.get('token') as string;

    this.buttonState = 'waiting';

    try {
      const response = await umbAuthContext.validateMfaCode(code, provider);
      if (response.error) {
        if (codeInput) {
          codeInput.error = true;
          codeInput.errorMessage = response.error;
        } else {
          this.error = response.error;
        }
        this.buttonState = 'failed';
        return;
      }

      this.buttonState = 'success';

      const returnPath = umbAuthContext.returnPath;
      if (returnPath) {
        location.href = returnPath;
      }

      this.dispatchEvent(
        new CustomEvent('umb-login-success', {bubbles: true, composed: true, detail: response.data})
      );
    } catch (e) {
      if (e instanceof Error) {
        this.error = e.message ?? 'Unknown error';
      } else {
        this.error = 'Unknown error';
      }
      this.buttonState = 'failed';
      this.dispatchEvent(new CustomEvent('umb-login-failed', {bubbles: true, composed: true, detail: e}));
    }
  }

  protected renderDefaultView() {
    return html`
      <uui-form>
        <form id="LoginForm" @submit=${this.handleSubmit}>
          <header id="header">
            <h1>
              <umb-localize key="login_2faTitle">One last step</umb-localize>
            </h1>

            <p>
              <umb-localize key="login_2faText">
                You have enabled 2-factor authentication and must verify your identity.
              </umb-localize>
            </p>
          </header>

          <!-- if there's only one provider active, it will skip this step! -->
          ${this.providers.length > 1
            ? html`
              <uui-form-layout-item label="@login_2faMultipleText">
                <uui-label id="providerLabel" for="provider" slot="label" required>
                  <umb-localize key="login_2faMultipleText">Please choose a 2-factor provider</umb-localize>
                </uui-label>
                <div class="uui-input-wrapper">
                  <uui-select id="provider" name="provider" .options=${this.providers} aria-required="true" required>
                  </uui-select>
                </div>
              </uui-form-layout-item>
            `
            : nothing}

          <uui-form-layout-item>
            <uui-label id="2facodeLabel" for="2facode" slot="label" required>
              <umb-localize key="login_2faCodeInput">Verification code</umb-localize>
            </uui-label>

            <uui-input
              autofocus
              id="2facode"
              type="text"
              name="token"
              inputmode="numeric"
              autocomplete="one-time-code"
              placeholder=${until(
                umbLocalizationContext.localize('login_2faCodeInputHelp'),
                'Please enter the verification code'
              )}
              aria-required="true"
              required
              required-message=${until(
                umbLocalizationContext.localize('login_2faCodeInputHelp'),
                'Please enter the verification code'
              )}
              style="width:100%;">
            </uui-input>
          </uui-form-layout-item>

          ${this.error ? html` <span class="text-danger">${this.error}</span> ` : nothing}

          <uui-button
            .state=${this.buttonState}
            button-style="success"
            look="primary"
            color="default"
            label=${until(umbLocalizationContext.localize('general_validate'), 'Validate')}
            type="submit"></uui-button>
        </form>
      </uui-form>

      <umb-back-to-login-button style="margin-top: var(--uui-size-space-6)"></umb-back-to-login-button>
    `;
  }

  protected async renderCustomView() {
    const view = umbAuthContext.twoFactorView;
    if (!view) return nothing;

    try {
      const customView = await loadCustomView<MfaCustomViewElement>(view);
      if (typeof customView === 'object') {
        customView.providers = this.providers.map((provider) => provider.value);
        customView.returnPath = umbAuthContext.returnPath;
      }
      return renderCustomView(customView);
    } catch (e) {
      const error = e instanceof Error ? e.message : 'Unknown error';
      console.group('[MFA login] Failed to load custom view');
      console.log('Element reference', this);
      console.log('Custom view', view);
      console.error('Failed to load custom view:', e);
      console.groupEnd();
      return html`<span class="text-danger">${error}</span>`;
    }
  }

  protected render() {
    return this.loading
      ? html`
        <uui-loader-bar></uui-loader-bar>`
      : umbAuthContext.twoFactorView
        ? until(this.renderCustomView(), html`
          <uui-loader-bar></uui-loader-bar>`)
        : this.renderDefaultView();
  }

  static styles = [
    css`
      #header {
        text-align: center;
      }

      #header h1 {
        font-weight: 400;
        font-size: var(--header-secondary-font-size);
        color: var(--uui-color-interactive);
        line-height: 1.2;
      }

      form {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-layout-2);
      }

      .uui-input-wrapper {
        background-color: var(--uui-color-surface);
      }

      uui-form-layout-item {
        margin: 0;
      }

      uui-input,
      uui-input-password {
        width: 100%;
        height: var(--input-height);
        border-radius: var(--uui-border-radius);
      }

      uui-input {
        width: 100%;
      }

      uui-button {
        width: 100%;
        --uui-button-padding-top-factor: 1.5;
        --uui-button-padding-bottom-factor: 1.5;
      }

      .text-danger {
        color: var(--uui-color-danger-standalone);
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-mfa-page': UmbMfaPageElement;
  }
}
