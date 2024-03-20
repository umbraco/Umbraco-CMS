import type {UUIButtonState, UUIInputPasswordElement} from '@umbraco-cms/backoffice/external/uui';
import {CSSResultGroup, css, html, nothing, customElement, property, query} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";

import { umbAuthContext } from '../../context/auth.context.js';

@customElement('umb-new-password-layout')
export default class UmbNewPasswordLayoutElement extends UmbLitElement {
  #passwordConfiguration = umbAuthContext.passwordConfiguration;
  #passwordPattern = '';

  @query('#password')
  passwordElement!: UUIInputPasswordElement;

  @query('#confirmPassword')
  confirmPasswordElement!: UUIInputPasswordElement;

  @property()
  state: UUIButtonState = undefined;

  @property()
  error: string = '';

  @property()
  userId: string = '';

  @property({ type: Boolean, attribute: 'is-invite' })
  isInvite = false;

  constructor() {
    super();

    // Build a pattern
    let pattern = '';
    if (this.#passwordConfiguration?.requireDigit) {
      pattern += '(?=.*\\d)';
    }
    if (this.#passwordConfiguration?.requireLowercase) {
      pattern += '(?=.*[a-z])';
    }
    if (this.#passwordConfiguration?.requireUppercase) {
      pattern += '(?=.*[A-Z])';
    }
    if (this.#passwordConfiguration?.requireNonLetterOrDigit) {
      pattern += '(?=.*\\W)';
    }
    pattern += `.{${this.#passwordConfiguration?.minimumPasswordLength ?? 10},}`;
    this.#passwordPattern = pattern;
  }

  #onSubmit(event: Event) {
    event.preventDefault();
    if (!this.#passwordConfiguration) return;

    const form = event.target as HTMLFormElement;

    this.passwordElement.setCustomValidity('');
    this.confirmPasswordElement.setCustomValidity('');

    if (!form) return;
    if (!form.checkValidity()) return;

    const formData = new FormData(form);
    const password = formData.get('password') as string;
    const passwordConfirm = formData.get('confirmPassword') as string;

    let passwordIsInvalid = false;

    if (this.#passwordConfiguration.minimumPasswordLength > 0 && password.length < this.#passwordConfiguration.minimumPasswordLength) {
      passwordIsInvalid = true;
    }

    if (this.#passwordConfiguration.requireNonLetterOrDigit) {
      const hasNonLetterOrDigit = /\W/.test(password);
      if (!hasNonLetterOrDigit) {
        passwordIsInvalid = true;
      }
    }

    if (this.#passwordConfiguration.requireDigit) {
      const hasDigit = /\d/.test(password);
      if (!hasDigit) {
        passwordIsInvalid = true;
      }
    }

    if (this.#passwordConfiguration.requireLowercase) {
      const hasLowercase = /[a-z]/.test(password);
      if (!hasLowercase) {
        passwordIsInvalid = true;
      }
    }

    if (this.#passwordConfiguration.requireUppercase) {
      const hasUppercase = /[A-Z]/.test(password);
      if (!hasUppercase) {
        passwordIsInvalid = true;
      }
    }

    if (passwordIsInvalid) {
      const passwordValidityText = this.localize.term(
        'errorHandling_errorInPasswordFormat',
        this.#passwordConfiguration.minimumPasswordLength,
        this.#passwordConfiguration.requireNonLetterOrDigit ? 1 : 0
      ) ?? 'The password does not meet the minimum requirements!';
      this.passwordElement.setCustomValidity(passwordValidityText);
      return;
    }

    if (password !== passwordConfirm) {
      const passwordValidityText = this.localize.term(
        'user_passwordMismatch'
      ) ?? "The confirmed password doesn't match the new password!";
      this.confirmPasswordElement.setCustomValidity(passwordValidityText);
      return;
    }

    this.dispatchEvent(new CustomEvent('submit', {detail: {password}}));
  }

  renderHeader() {
    if (this.isInvite) {
      return html`
        <h1>Hi!</h1>
        <span>
          <umb-localize key="user_userinviteWelcomeMessage">
            Welcome to Umbraco! Just need to get your password setup and then you're good to go
          </umb-localize>
        </span>
      `;
    } else {
      return html`
        <h1>
          <umb-localize key="user_newPassword">New password</umb-localize>
        </h1>
        <span>
            <umb-localize key="login_setPasswordInstruction">Please provide a new password.</umb-localize>
        </span>
      `;
    }
  }

  render() {
    return html`
      <uui-form>
        <form id="LoginForm" name="login" @submit=${this.#onSubmit}>
          <header id="header">${this.renderHeader()}</header>
          <uui-form-layout-item>
            <uui-label id="passwordLabel" for="password" slot="label" required>
              <umb-localize key="user_newPassword">New password</umb-localize>
            </uui-label>
            <uui-input-password
              type="password"
              id="password"
              name="password"
              autocomplete="new-password"
              pattern="${this.#passwordPattern}"
              .minlength=${this.#passwordConfiguration?.minimumPasswordLength}
              .minlengthMessage=${this.localize.term('user_passwordMinLength', this.#passwordConfiguration?.minimumPasswordLength ?? 10)}
              .label=${this.localize.term('user_newPassword')}
              required
              required-message=${this.localize.term('user_passwordIsBlank')}>
            </uui-input-password>
          </uui-form-layout-item>

          <uui-form-layout-item>
            <uui-label id="confirmPasswordLabel" for="confirmPassword" slot="label" required>
              <umb-localize key="user_confirmNewPassword">Confirm new password</umb-localize>
            </uui-label>
            <uui-input-password
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              autocomplete="new-password"
              pattern="${this.#passwordPattern}"
              .minlength=${this.#passwordConfiguration?.minimumPasswordLength}
              .minlengthMessage=${this.localize.term('user_passwordMinLength', this.#passwordConfiguration?.minimumPasswordLength ?? 10)}
              .label=${this.localize.term('user_confirmNewPassword')}
              required
              required-message=${this.localize.term('general_required')}></uui-input-password>
          </uui-form-layout-item>

          ${this.#renderErrorMessage()}

          <uui-button
            type="submit"
            label=${this.localize.term('general_continue')}
            look="primary"
            color="default"
            .state=${this.state}></uui-button>
        </form>
      </uui-form>

      <umb-back-to-login-button style="margin-top: var(--uui-size-space-6)"></umb-back-to-login-button>
    `;
  }

  #renderErrorMessage() {
    if (!this.error || this.state !== 'failed') return nothing;

    return html`<span class="text-danger">${this.error}</span>`;
  }

  static styles: CSSResultGroup = [
    css`
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

      #header h1 {
        margin: 0;
        font-weight: 400;
        font-size: var(--header-secondary-font-size);
        color: var(--uui-color-interactive);
        line-height: 1.2;
      }

      form {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-5);
      }

      uui-form-layout-item {
        margin: 0;
      }

      uui-input-password {
        width: 100%;
        height: var(--input-height);
        border-radius: var(--uui-border-radius);
      }

      uui-button {
        width: 100%;
        margin-top: var(--uui-size-space-5);
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
    'umb-new-password-layout': UmbNewPasswordLayoutElement;
  }
}
