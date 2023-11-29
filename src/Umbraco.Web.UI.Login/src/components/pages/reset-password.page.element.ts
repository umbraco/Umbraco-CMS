import type {UUIButtonState} from '@umbraco-ui/uui';
import {CSSResultGroup, LitElement, css, html, nothing} from 'lit';
import {customElement, state} from 'lit/decorators.js';
import {until} from 'lit/directives/until.js';

import {umbAuthContext} from '../../context/auth.context.js';
import {umbLocalizationContext} from '../../external/localization/localization-context.js';

@customElement('umb-reset-password-page')
export default class UmbResetPasswordPageElement extends LitElement {
  @state()
  resetCallState: UUIButtonState = undefined;

  @state()
  error = '';

  #handleResetSubmit = async (e: SubmitEvent) => {
    e.preventDefault();
    const form = e.target as HTMLFormElement;

    if (!form) return;
    if (!form.checkValidity()) return;

    const formData = new FormData(form);
    const username = formData.get('email') as string;

    this.resetCallState = 'waiting';
    const response = await umbAuthContext.resetPassword(username);
    this.resetCallState = response.status === 200 ? 'success' : 'failed';
    this.error = response.error || '';
  };

  #renderResetPage() {
    return html`
      <uui-form>
        <form id="LoginForm" name="login" @submit="${this.#handleResetSubmit}">
          <header id="header">
            <h1>
              <umb-localize key="login_forgottenPassword">Forgotten password?</umb-localize>
            </h1>
            <span>
							<umb-localize key="login_forgottenPasswordInstruction"
              >An email will be sent to the address specified with a link to reset your password</umb-localize
              >
						</span>
          </header>

          <uui-form-layout-item>
            <uui-label for="email" slot="label" required>
              <umb-localize key="general_email">Email</umb-localize>
            </uui-label>
            <uui-input
              type="email"
              id="email"
              name="email"
              .label=${until(umbLocalizationContext.localize('general_email', undefined, 'Email'))}
              required
              required-message=${until(
                umbLocalizationContext.localize('general_required', undefined, 'Required')
              )}></uui-input>
          </uui-form-layout-item>

          ${this.#renderErrorMessage()}

          <uui-button
            type="submit"
            .label=${until(umbLocalizationContext.localize('general_submit', undefined, 'Submit'))}
            look="primary"
            color="default"
            .state=${this.resetCallState}></uui-button>
        </form>
      </uui-form>

      <umb-back-to-login-button style="margin-top: var(--uui-size-space-6)"></umb-back-to-login-button>
    `;
  }

  #renderErrorMessage() {
    if (!this.error || this.resetCallState !== 'failed') return nothing;

    return html`<span class="text-danger">${this.error}</span>`;
  }

  #renderConfirmationPage() {
    return html`
      <umb-confirmation-layout
        header=${until(umbLocalizationContext.localize('login_forgottenPassword', undefined, 'Forgotten password?'))}
        message=${until(
          umbLocalizationContext.localize(
            'login_requestPasswordResetConfirmation',
            undefined,
            'An email with password reset instructions will be sent to the specified address if it matched our records'
          )
        )}>
      </umb-confirmation-layout>
    `;
  }

  render() {
    return this.resetCallState === 'success' ? this.#renderConfirmationPage() : this.#renderResetPage();
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
        gap: var(--uui-size-layout-2);
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

      #resend {
        display: inline-flex;
        font-size: 14px;
        align-self: center;
        gap: var(--uui-size-space-1);
      }

      #resend a {
        color: var(--uui-color-selected);
        font-weight: 600;
        text-decoration: none;
      }

      #resend a:hover {
        color: var(--uui-color-interactive-emphasis);
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-reset-password-page': UmbResetPasswordPageElement;
  }
}
