import type {UUIButtonState} from '@umbraco-ui/uui';
import {LitElement, html} from 'lit';
import {customElement, state} from 'lit/decorators.js';
import {until} from 'lit/directives/until.js';

import {umbAuthContext} from '../../context/auth.context.js';
import {umbLocalizationContext} from '../../external/localization/localization-context.js';

@customElement('umb-new-password-page')
export default class UmbNewPasswordPageElement extends LitElement {
  @state()
  state: UUIButtonState = undefined;

  @state()
  page: 'new' | 'done' | 'error' = 'new';

  @state()
  error = '';

  @state()
  userId = '';

  @state()
  resetCode = '';

  @state()
  loading = true;

  constructor() {
    super();
    this.#init();
  }

  async #init() {
    const urlParams = new URLSearchParams(window.location.search);
    const resetCode = urlParams.get('resetCode');
    const userId = urlParams.get('userId');

    if (!userId || !resetCode) {
      this.page = 'error';
      this.loading = false;
      return;
    }

    this.resetCode = resetCode;
    this.userId = userId;

    const verifyResponse = await umbAuthContext.validatePasswordResetCode(this.userId, this.resetCode);

    if (verifyResponse.error) {
      this.page = 'error';
      this.error = verifyResponse.error;
      this.loading = false;
      return;
    }

    umbAuthContext.passwordConfiguration = verifyResponse.data?.passwordConfiguration;

    this.loading = false;
  }

  async #onSubmit(event: CustomEvent) {
    event.preventDefault();
    const password = event.detail.password;

    this.state = 'waiting';
    const response = await umbAuthContext.newPassword(password, this.resetCode, this.userId);

    if (response.status === 204) {
      this.state = 'success';
      this.page = 'done';
      this.error = '';
      return;
    }

    this.state = 'failed';
    this.error = response.error ?? 'Could not set new password';
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
            message=${this.error ?? until(
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
    return this.loading ? html`<uui-loader-bar></uui-loader-bar>` : this.#renderRoutes();
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-new-password-page': UmbNewPasswordPageElement;
  }
}
