import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, type CSSResultGroup, html, nothing, when, customElement, property, queryAssignedElements, state } from '@umbraco-cms/backoffice/external/lit';

import { UMB_AUTH_CONTEXT } from '../../contexts';

@customElement('umb-login-page')
export default class UmbLoginPageElement extends UmbLitElement {
  @property({type: Boolean, attribute: 'username-is-email'})
  usernameIsEmail = false;

  @queryAssignedElements({flatten: true})
  protected slottedElements?: HTMLFormElement[];

  @property({type: Boolean, attribute: 'allow-password-reset'})
  allowPasswordReset = false;

  @state()
  private _loginState?: UUIButtonState;

  @state()
  private _loginError = '';

  @state()
  supportPersistLogin = false;

  #formElement?: HTMLFormElement;

  #authContext?: typeof UMB_AUTH_CONTEXT.TYPE;

  constructor() {
    super();

    this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
      this.#authContext = authContext;
      this.supportPersistLogin = authContext.supportsPersistLogin;
    });
  }

  async #onSlotChanged() {
    this.#formElement = this.slottedElements?.find((el) => el.id === 'umb-login-form');

    if (!this.#formElement) return;

    // We need to listen for the enter key to submit the form, because the uui-button does not support the native input fields submit event
    this.#formElement.addEventListener('keypress', (e) => {
      if (e.key === 'Enter') {
        this.#onSubmitClick();
      }
    });
    this.#formElement.onsubmit = this.#handleSubmit;
  }

  #handleSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    if (!this.#authContext) return;

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const formData = new FormData(form);

    const username = formData.get('username') as string;
    const password = formData.get('password') as string;
    const persist = formData.has('persist');

    if (!username || !password) {
      this._loginError = this.localize.term('auth_userFailedLogin');
      this._loginState = 'failed';
      return;
    }

    this._loginState = 'waiting';

    const response = await this.#authContext.login({
      username,
      password,
      persist,
    });

    this._loginError = response.error || '';
    this._loginState = response.error ? 'failed' : 'success';

    // Check for 402 status code indicating that MFA is required
    if (response.status === 402) {
      this.#authContext.isMfaEnabled = true;
      if (response.twoFactorView) {
        this.#authContext.twoFactorView = response.twoFactorView;
      }
      if (response.twoFactorProviders) {
        this.#authContext.mfaProviders = response.twoFactorProviders;
      }

      this.dispatchEvent(new CustomEvent('umb-login-flow', {composed: true, detail: {flow: 'mfa'}}));
      return;
    }

    if (response.error) {
      return;
    }

    const returnPath = this.#authContext.returnPath;

    if (returnPath) {
      location.href = returnPath;
    }
  };

  get #greetingLocalizationKey() {
    return [
      'auth_greeting0',
      'auth_greeting1',
      'auth_greeting2',
      'auth_greeting3',
      'auth_greeting4',
      'auth_greeting5',
      'auth_greeting6',
    ][new Date().getDay()];
  }

  #onSubmitClick = () => {
    this.#formElement?.requestSubmit();
  };

  render() {
    return html`
      <header id="header">
        <h1 id="greeting">
          <umb-localize .key=${this.#greetingLocalizationKey}>Welcome</umb-localize>
        </h1>
        <slot name="subheadline"></slot>
      </header>
      <slot @slotchange=${this.#onSlotChanged}></slot>
      <div id="secondary-actions">
        ${when(
          this.supportPersistLogin,
          () => html`
            <uui-form-layout-item>
              <uui-checkbox
                name="persist"
                .label=${this.localize.term('auth_rememberMe')}>
                <umb-localize key="auth_rememberMe">Remember me</umb-localize>
              </uui-checkbox>
            </uui-form-layout-item>`
        )}
        ${when(
          this.allowPasswordReset,
          () =>
            html`
              <button type="button" id="forgot-password" @click=${this.#handleForgottenPassword}>
                <umb-localize key="auth_forgottenPassword">Forgotten password?</umb-localize>
              </button>`
        )}
      </div>
      <uui-button
        type="submit"
        id="umb-login-button"
        look="primary"
        @click=${this.#onSubmitClick}
        .label=${this.localize.term('auth_login')}
        color="default"
        .state=${this._loginState}></uui-button>

      ${this.#renderErrorMessage()}
    `;
  }

  #renderErrorMessage() {
    if (!this._loginError || this._loginState !== 'failed') return nothing;

    return html`<span class="text-error text-danger">${this._loginError}</span>`;
  }

  #handleForgottenPassword() {
    this.dispatchEvent(new CustomEvent('umb-login-flow', {composed: true, detail: {flow: 'reset'}}));
  }

  static styles: CSSResultGroup = [
    css`
      :host {
        display: flex;
        flex-direction: column;
      }

      #header {
        text-align: center;
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-5);
      }

      #header span {
        color: var(--uui-color-text-alt); /* TODO Change to uui color when uui gets a muted text variable */
        font-size: 14px;
      }

      #greeting {
        color: var(--uui-color-interactive);
        text-align: center;
        font-weight: 400;
        font-size: var(--header-font-size);
        margin: 0 0 var(--uui-size-layout-1);
        line-height: 1.2;
      }

      #umb-login-button {
        margin-top: var(--uui-size-space-4);
        width: 100%;
      }

      #forgot-password {
        cursor: pointer;
        background: none;
        border: 0;
        height: 1rem;
        color: var(--uui-color-text-alt); /* TODO Change to uui color when uui gets a muted text variable */
        gap: var(--uui-size-space-1);
        align-self: center;
        text-decoration: none;
        display: inline-flex;
        line-height: 1;
        font-size: 14px;
        font-family: var(--uui-font-family),sans-serif;
        margin-left: auto;
        margin-bottom: var(--uui-size-space-3);
      }

      #forgot-password:hover {
        color: var(--uui-color-interactive-emphasis);
      }

      .text-error {
        margin-top: var(--uui-size-space-4);
      }

      .text-danger {
        color: var(--uui-color-danger-standalone);
      }

      #secondary-actions {
        display: flex;
        align-items: center;
        justify-content: space-between;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-login-page': UmbLoginPageElement;
  }
}
