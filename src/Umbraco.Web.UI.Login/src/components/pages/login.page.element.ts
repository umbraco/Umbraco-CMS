import type {UUIButtonState} from '@umbraco-ui/uui';
import {css, CSSResultGroup, html, LitElement, nothing} from 'lit';
import {customElement, property, queryAssignedElements, state} from 'lit/decorators.js';
import {when} from 'lit/directives/when.js';
import {until} from 'lit/directives/until.js';

import {umbAuthContext} from '../../context/auth.context.js';
import {umbLocalizationContext} from '../../external/localization/localization-context.js';

@customElement('umb-login-page')
export default class UmbLoginPageElement extends LitElement {
  @property({type: Boolean, attribute: 'username-is-email'})
  usernameIsEmail = false;

  @queryAssignedElements({flatten: true})
  protected slottedElements?: HTMLFormElement[];

  @property({type: Boolean, attribute: 'allow-password-reset'})
  allowPasswordReset = false;

  @state()
  private _loginState: UUIButtonState = undefined;

  @state()
  private _loginError = '';

  @state()
  private get disableLocalLogin() {
    return umbAuthContext.disableLocalLogin;
  }

  #formElement?: HTMLFormElement;

  async #onSlotChanged() {
    this.#formElement = this.slottedElements?.find((el) => el.id === 'umb-login-form');

    if (!this.#formElement) return;

    this.#formElement.onsubmit = this.#handleSubmit;
  }

  #handleSubmit = async (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    if (!form.checkValidity()) return;

    const formData = new FormData(form);

    const username = formData.get('username') as string;
    const password = formData.get('password') as string;
    const persist = formData.has('persist');

    this._loginState = 'waiting';

    const response = await umbAuthContext.login({
      username,
      password,
      persist,
    });

    this._loginError = response.error || '';
    this._loginState = response.error ? 'failed' : 'success';

    // Check for 402 status code indicating that MFA is required
    if (response.status === 402) {
      umbAuthContext.isMfaEnabled = true;
      if (response.twoFactorView) {
        umbAuthContext.twoFactorView = response.twoFactorView;
      }

      this.dispatchEvent(new CustomEvent('umb-login-flow', {composed: true, detail: {flow: 'mfa'}}));
      return;
    }

    if (response.error) {
      this.dispatchEvent(new CustomEvent('umb-login-failed', {bubbles: true, composed: true, detail: response}));
      return;
    }

    const returnPath = umbAuthContext.returnPath;

    if (returnPath) {
      location.href = returnPath;
    }

    this.dispatchEvent(new CustomEvent('umb-login-success', {bubbles: true, composed: true, detail: response.data}));
  };

  get #greetingLocalizationKey() {
    return [
      'login_greeting0',
      'login_greeting1',
      'login_greeting2',
      'login_greeting3',
      'login_greeting4',
      'login_greeting5',
      'login_greeting6',
    ][new Date().getDay()];
  }

  #onSubmitClick = () => {
    this.#formElement?.requestSubmit();
  };

  render() {
    return html`
      <header id="header">
        <h1 id="greeting">
          <umb-localize .key=${this.#greetingLocalizationKey}></umb-localize>
        </h1>
        <slot name="subheadline"></slot>
      </header>
      ${this.disableLocalLogin
        ? nothing
        : html`
          <slot @slotchange=${this.#onSlotChanged}></slot>
          <div id="secondary-actions">
            ${when(
              umbAuthContext.supportsPersistLogin,
              () => html`
                <uui-form-layout-item>
                  <uui-checkbox
                    name="persist"
                    .label=${until(umbLocalizationContext.localize('user_rememberMe', undefined, 'Remember me'))}>
                    <umb-localize key="user_rememberMe">Remember me</umb-localize>
                  </uui-checkbox>
                </uui-form-layout-item>`
            )}
            ${when(
              this.allowPasswordReset,
              () =>
                html`
                  <button type="button" id="forgot-password" @click=${this.#handleForgottenPassword}>
                    <umb-localize key="login_forgottenPassword">Forgotten password?</umb-localize>
                  </button>`
            )}
          </div>
          <uui-button
            type="submit"
            id="umb-login-button"
            look="primary"
            @click=${this.#onSubmitClick}
            .label=${until(umbLocalizationContext.localize('general_login', undefined, 'Login'), 'Login')}
            color="default"
            .state=${this._loginState}></uui-button>

          ${this.#renderErrorMessage()}
        `}
      <umb-external-login-providers-layout .showDivider=${!this.disableLocalLogin}>
        <slot name="external"></slot>
      </umb-external-login-providers-layout>
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
        font-family: var(--uui-font-family);
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
