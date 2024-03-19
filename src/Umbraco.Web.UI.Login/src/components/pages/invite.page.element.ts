import type { UUIButtonState } from '@umbraco-ui/uui';
import { LitElement, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { until } from 'lit/directives/until.js';

import { umbAuthContext } from '../../context/auth.context.js';
import { umbLocalizationContext } from '../../external/localization/localization-context.js';

@customElement('umb-invite-page')
export default class UmbInvitePageElement extends LitElement {
  #token = '';
  #userId = '';

  @state()
  state: UUIButtonState = undefined;

  @state()
  error = '';

  @state()
  loading = true;

  constructor() {
    super();
    this.#init();
  }

  async #init() {
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get('inviteCode');
    const userId = urlParams.get('userId');

    if (!token || !userId) {
      this.error = 'The invite has expired or is invalid';
      this.loading = false;
      return;
    }

    this.#token = token;
    this.#userId = userId;

    const response = await umbAuthContext.validateInviteCode(this.#token, this.#userId);

    if (response.error) {
      this.error = response.error;
      this.loading = false;
      return;
    }

    umbAuthContext.passwordConfiguration = response.data?.passwordConfiguration;
    this.loading = false;
  }

  async #onSubmit(event: CustomEvent) {
    event.preventDefault();
    const password = event.detail.password;

    if (!password) return;

    this.state = 'waiting';
    const response = await umbAuthContext.newInvitedUserPassword(password, this.#token, this.#userId);

    if (response.error) {
      this.error = response.error;
      this.state = 'failed';
      return;
    }

    this.state = 'success';
    window.location.href = umbAuthContext.returnPath;
  }

  render() {
    return this.loading ? html`<uui-loader-bar></uui-loader-bar>` : (
      this.error
        ? html`
          <umb-error-layout
            header=${until(umbLocalizationContext.localize('general_error', undefined, 'Error'))}
            message=${this.error ?? until(
              umbLocalizationContext.localize('errors_defaultError', undefined, 'An unknown failure has occured')
            )}>
          </umb-error-layout>`
      : html`
        <umb-new-password-layout
          @submit=${this.#onSubmit}
          is-invite
          .userId=${this.#userId}
          .state=${this.state}
          .error=${this.error}></umb-new-password-layout>`
    );
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-invite-page': UmbInvitePageElement;
  }
}
