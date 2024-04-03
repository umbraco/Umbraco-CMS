import type {UUIButtonState} from '@umbraco-cms/backoffice/external/uui';
import {type CSSResultGroup, css, html, nothing, customElement, state} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";

import { UMB_AUTH_CONTEXT } from '../../contexts';

@customElement('umb-reset-password-page')
export default class UmbResetPasswordPageElement extends UmbLitElement {
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
    const authContext = await this.getContext(UMB_AUTH_CONTEXT);
    const response = await authContext.resetPassword(username);
    this.resetCallState = response.error ? 'failed' : 'success';
    this.error = response.error || '';
  };

  #renderResetPage() {
    return html`
      <uui-form>
        <form id="LoginForm" name="login" @submit="${this.#handleResetSubmit}">
          <header id="header">
            <h1>
              <umb-localize key="auth_forgottenPassword">Forgotten password?</umb-localize>
            </h1>
            <span>
							<umb-localize key="auth_forgottenPasswordInstruction">
                An email will be sent to the address specified with a link to reset your password
              </umb-localize>
						</span>
          </header>

          <uui-form-layout-item>
            <uui-label for="email" slot="label" required>
              <umb-localize key="auth_email">Email</umb-localize>
            </uui-label>
            <uui-input
              type="email"
              id="email"
              name="email"
              .label=${this.localize.term('auth_email')}
              required
              required-message=${this.localize.term('auth_required')}>
            </uui-input>
          </uui-form-layout-item>

          ${this.#renderErrorMessage()}

          <uui-button
            type="submit"
            .label=${this.localize.term('auth_submit')}
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
        header=${this.localize.term('auth_forgottenPassword')}
        message=${this.localize.term('auth_requestPasswordResetConfirmation')}>
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
