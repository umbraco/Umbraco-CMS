import type {UUIButtonState} from '@umbraco-cms/backoffice/external/uui';
import {html, customElement, state} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import {umbAuthContext} from '../../context/auth.context.js';

@customElement('umb-new-password-page')
export default class UmbNewPasswordPageElement extends UmbLitElement {
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
            header=${this.localize.term('general_error')}
            message=${this.error ?? this.localize.term('errors_defaultError')}>
          </umb-error-layout>`;
      case 'done':
        return html`
          <umb-confirmation-layout
            header=${this.localize.term('general_success')}
            message=${this.localize.term('login_setPasswordConfirmation')}>
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
