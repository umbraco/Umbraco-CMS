import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import { UMB_AUTH_CONTEXT } from "../../contexts";

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

  #authContext?: typeof UMB_AUTH_CONTEXT.TYPE;

  constructor() {
    super();
    this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
      this.#authContext = authContext;
      this.#init();
    });
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

    if (!this.#authContext) return;

    this.resetCode = resetCode;
    this.userId = userId;

    const verifyResponse = await this.#authContext.validatePasswordResetCode(this.userId, this.resetCode);

    if (verifyResponse.error) {
      this.page = 'error';
      this.error = verifyResponse.error;
      this.loading = false;
      return;
    }

    if (!verifyResponse.passwordConfiguration) {
      this.page = 'error';
      this.error = 'Password configuration is missing';
      this.loading = false;
      return;
    }

    this.#authContext.passwordConfiguration = verifyResponse.passwordConfiguration;

    this.loading = false;
  }

  async #onSubmit(event: CustomEvent) {
    event.preventDefault();

    this.error = '';

    if (!this.#authContext) return;

    const password = event.detail.password;

    this.state = 'waiting';
    const response = await this.#authContext.newPassword(password, this.resetCode, this.userId);

    if (response.error) {
      this.state = 'failed';
      this.error = response.error;
      return;
    }

    this.state = 'success';
    this.page = 'done';
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
            header=${this.localize.term('auth_error')}
            message=${this.error ?? this.localize.term('auth_defaultError')}>
          </umb-error-layout>`;
      case 'done':
        return html`
          <umb-confirmation-layout
            header=${this.localize.term('auth_success')}
            message=${this.localize.term('auth_setPasswordConfirmation')}>
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
