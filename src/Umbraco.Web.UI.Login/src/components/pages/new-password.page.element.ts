import type {UUIButtonState, UUIInputPasswordElement} from '@umbraco-ui/uui';
import {LitElement, html} from 'lit';
import {customElement, query, state} from 'lit/decorators.js';
import {until} from 'lit/directives/until.js';

import {umbAuthContext} from '../../context/auth.context.js';
import {umbLocalizationContext} from '../../external/localization/localization-context.js';

@customElement('umb-new-password-page')
export default class UmbNewPasswordPageElement extends LitElement {
  @query('#confirmPassword')
  confirmPasswordElement!: UUIInputPasswordElement;

  @state()
  state: UUIButtonState = undefined;

  @state()
  page: 'new' | 'done' | 'error' = 'new';

  @state()
  error = '';

  @state()
  userId: string | null = null;

  @state()
  resetCode: string | null = null;

  constructor() {
    super();

    const urlParams = new URLSearchParams(window.location.search);
    this.resetCode = urlParams.get('resetCode');
    this.userId = urlParams.get('userId');

    if (!this.userId || !this.resetCode) {
      this.page = 'error';
    }
  }

  async #onSubmit(event: CustomEvent) {
    event.preventDefault();
    const urlParams = new URLSearchParams(window.location.search);
    const resetCode = urlParams.get('resetCode');
    const userId = urlParams.get('userId');
    const password = event.detail.password;

    if (!resetCode || !userId) return;

    this.state = 'waiting';
    const response = await umbAuthContext.newPassword(password, resetCode, userId);
    this.state = response.status === 200 ? 'success' : 'failed';
    this.page = response.status === 200 ? 'done' : 'new';
    this.error = response.error || '';
  }

  #renderRoutes() {
    switch (this.page) {
      case 'new':
        return html`
          <umb-new-password-layout
            @submit=${this.#onSubmit}
            .userId=${this.userId!}
            .state=${this.state}
            .error=${this.error}></umb-new-password-layout>`;
      case 'error':
        return html`
          <umb-error-layout
            header=${until(umbLocalizationContext.localize('general_error', undefined, 'Error'))}
            message=${until(
              umbLocalizationContext.localize('errors_defaultError', undefined, 'An unknown failure has occured')
            )}>
          </umb-error-layout>`;
      case 'done':
        return html`
          <umb-confirmation-layout
            header=${until(umbLocalizationContext.localize('general_success', undefined, 'Success'))}
            message=${until(
              umbLocalizationContext.localize(
                'login_setPasswordConfirmation',
                undefined,
                'Your new password has been set and you may now use it to log in.'
              )
            )}>
          </umb-confirmation-layout>`;
    }
  }

  render() {
    return this.userId && this.resetCode
      ? this.#renderRoutes()
      : html`
        <umb-error-layout
          header=${until(umbLocalizationContext.localize('general_error', undefined, 'Error'))}
          message=${until(
            umbLocalizationContext.localize('errors_defaultError', undefined, 'An unknown failure has occured')
          )}>
        </umb-error-layout>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-new-password-page': UmbNewPasswordPageElement;
  }
}
