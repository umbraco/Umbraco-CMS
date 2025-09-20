import type { UUIButtonState } from '@umbraco-ui/uui';
import { LitElement, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { until } from 'lit/directives/until.js';

import { umbAuthContext } from '../../context/auth.context.js';
import { umbLocalizationContext } from '../../external/localization/localization-context.js';

@customElement('umb-invite-page')
export default class UmbInvitePageElement extends LitElement {
  @state()
  state: UUIButtonState = undefined;

  @state()
  error = '';

  @state()
  invitedUser?: any;

  protected async firstUpdated(_changedProperties: any) {
    super.firstUpdated(_changedProperties);

    const response = await umbAuthContext.getInvitedUser();

    if (!response.user?.id) {
      // The login page should already have redirected the user to an error page. They should never get here.
      this.error = 'No invited user found';
      return;
    }

    this.invitedUser = response.user;
  }

  async #onSubmit(event: CustomEvent) {
    event.preventDefault();
    const password = event.detail.password;

    if (!password) return;

    this.state = 'waiting';
    const response = await umbAuthContext.newInvitedUserPassword(password);

    if (response.error) {
      this.error = response.error;
      this.state = 'failed';
      return;
    }

    this.state = 'success';
    window.location.href = umbAuthContext.returnPath;
  }

  render() {
    return this.invitedUser
      ? html`
        <umb-new-password-layout
          @submit=${this.#onSubmit}
          .userId=${this.invitedUser.id}
          .userName=${this.invitedUser.name}
          .state=${this.state}
          .error=${this.error}></umb-new-password-layout>`
      : this.error
        ? html`
          <umb-error-layout
            .header=${until(umbLocalizationContext.localize('general_error', undefined, 'Error'))}
            .message=${this.error}></umb-error-layout>`
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
    'umb-invite-page': UmbInvitePageElement;
  }
}
