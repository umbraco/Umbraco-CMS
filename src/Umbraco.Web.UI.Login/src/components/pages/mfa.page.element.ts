import type {UUIButtonState, UUIInputElement} from '@umbraco-cms/backoffice/external/uui';
import {css, html, nothing, customElement, state, until} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";

import { loadCustomView, renderCustomView } from '../../utils/load-custom-view.function.js';
import { UMB_AUTH_CONTEXT } from "../../contexts";

type MfaCustomViewElement = HTMLElement & {
  providers?: string[];
  returnPath?: string;
};

@customElement('umb-mfa-page')
export default class UmbMfaPageElement extends UmbLitElement {
  @state()
  protected providers: Array<{ name: string; value: string; selected: boolean }> = [];

  @state()
  private buttonState?: UUIButtonState;

  @state()
  private error: string | null = null;

  #authContext?: typeof UMB_AUTH_CONTEXT.TYPE;

  constructor() {
    super();
    this.consumeContext(UMB_AUTH_CONTEXT, authContext => {
      this.#authContext = authContext;
      this.#loadProviders();
    });
  }

  #loadProviders() {
    this.providers = this.#authContext?.mfaProviders.map((provider) => ({name: provider, value: provider, selected: false})) ?? [];

    if (this.providers.length) {
      this.providers[0].selected = true;
    } else {
      this.error = 'Error: No providers available';
    }
  }

  async #handleSubmit(e: SubmitEvent) {
    e.preventDefault();

    if (!this.#authContext) return;

    this.error = null;

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const codeInput = form.elements.namedItem('mfacode') as UUIInputElement;

    if (codeInput) {
      codeInput.error = false;
      codeInput.errorMessage = '';
      codeInput.setCustomValidity('');
    }

    if (!form.checkValidity()) return;

    const formData = new FormData(form);

    let provider = formData.get('provider') as string;

    // If no provider given, use the first one (there probably is only one anyway)
    if (!provider) {
      // If there are no providers, we can't continue
      if (!this.providers.length) {
        this.error = 'No providers available';
        return;
      }

      provider = this.providers[0].value;
    }

    if (!provider) {
      this.error = 'No provider selected';
      return;
    }

    const code = formData.get('token') as string;

    this.buttonState = 'waiting';

    const response = await this.#authContext.validateMfaCode(code, provider);
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

    const returnPath = this.#authContext.returnPath;
    if (returnPath) {
      location.href = returnPath;
    }
  }

  protected renderDefaultView() {
    return html`
      <uui-form>
        <form id="LoginForm" @submit=${this.#handleSubmit} novalidate>
          <header id="header">
            <h1>
              <umb-localize key="auth_mfaTitle">One last step</umb-localize>
            </h1>

            <p>
              <umb-localize key="auth_mfaText">
                You have enabled 2-factor authentication and must verify your identity.
              </umb-localize>
            </p>
          </header>

          <!-- if there's only one provider active, it will skip this step! -->
          ${this.providers.length > 1
            ? html`
              <uui-form-layout-item>
                <uui-label id="providerLabel" for="provider" slot="label" required>
                  <umb-localize key="auth_mfaMultipleText">Please choose a 2-factor provider</umb-localize>
                </uui-label>
                <uui-select label=${this.localize.term('auth_mfaMultipleText')} id="provider" name="provider" .options=${this.providers} aria-required="true" required></uui-select>
              </uui-form-layout-item>
            `
            : nothing}

          <uui-form-layout-item>
            <uui-label id="mfacodeLabel" for="mfacode" slot="label" required>
              <umb-localize key="auth_mfaCodeInput">Verification code</umb-localize>
            </uui-label>

            <uui-input
              autofocus
              id="mfacode"
              type="text"
              name="token"
              inputmode="numeric"
              autocomplete="one-time-code"
              placeholder=${this.localize.term('auth_mfaCodeInputHelp')}
              aria-required="true"
              required
              required-message=${this.localize.term('auth_mfaCodeInputHelp')}
              label=${this.localize.term('auth_mfaCodeInput')}
              style="width:100%;">
            </uui-input>
          </uui-form-layout-item>

          ${this.error ? html` <span class="text-danger">${this.error}</span> ` : nothing}

          <uui-button
            .state=${this.buttonState}
            button-style="success"
            look="primary"
            color="default"
            label=${this.localize.term('auth_validate')}
            type="submit"></uui-button>
        </form>
      </uui-form>

      <umb-back-to-login-button style="margin-top: var(--uui-size-space-6)"></umb-back-to-login-button>
    `;
  }

  protected async renderCustomView() {
    const view = this.#authContext?.twoFactorView;
    if (!view) return nothing;

    try {
      const customView = await loadCustomView<MfaCustomViewElement>(view);
      if (typeof customView === 'object') {
        customView.providers = this.providers.map((provider) => provider.value);
        customView.returnPath = this.#authContext?.returnPath ?? '';
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
    return this.#authContext?.twoFactorView
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

      #provider {
        width: 100%;
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
