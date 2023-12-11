import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { until } from 'lit/directives/until.js';

import { umbAuthContext } from './context/auth.context.js';
import { umbLocalizationContext } from './external/localization/localization-context.js';
import { UmbLocalizeElement } from './external/localization/localize.element.js';
import type { UmbLoginInputElement } from './components/login-input.element.js';
import type { InputType, UUIFormLayoutItemElement, UUILabelElement } from '@umbraco-ui/uui';

import authStyles from './auth-styles.css?inline';

const createInput = (opts: {
  id: string;
  type: InputType;
  name: string;
  autocomplete: AutoFill;
  requiredMessage: string;
  label: string;
  inputmode: string;
}) => {
  const input = document.createElement('umb-login-input');
  input.type = opts.type;
  input.name = opts.name;
  input.autocomplete = opts.autocomplete;
  input.id = opts.id;
  input.required = true;
  input.requiredMessage = opts.requiredMessage;
  input.label = opts.label;
  input.spellcheck = false;
  input.inputMode = opts.inputmode;

  return input;
};

const createLabel = (opts: { forId: string; localizeAlias: string }) => {
  const label = document.createElement('uui-label');
  const umbLocalize = document.createElement('umb-localize') as UmbLocalizeElement;
  umbLocalize.key = opts.localizeAlias;
  label.for = opts.forId;
  label.appendChild(umbLocalize);

  return label;
};

const createFormLayoutItem = (label: UUILabelElement, input: UmbLoginInputElement) => {
  const formLayoutItem = document.createElement('uui-form-layout-item') as UUIFormLayoutItemElement;
  formLayoutItem.appendChild(label);
  formLayoutItem.appendChild(input);

  return formLayoutItem;
};

const createForm = (elements: HTMLElement[]) => {
  const styles = document.createElement('style');
  styles.innerHTML = authStyles;
  const form = document.createElement('form');
  form.id = 'umb-login-form';
  form.name = 'login-form';
  form.noValidate = true;

  elements.push(styles);
  elements.forEach((element) => form.appendChild(element));

  return form;
};

@customElement('umb-auth')
export default class UmbAuthElement extends LitElement {
  /**
   * Disables the local login form and only allows external login providers.
   *
   * @attr disable-local-login
   */
  @property({type: Boolean, attribute: 'disable-local-login'})
  set disableLocalLogin(value: boolean) {
    umbAuthContext.disableLocalLogin = value;
  }

  @property({attribute: 'background-image'})
  backgroundImage?: string;

  @property({attribute: 'logo-image'})
  logoImage?: string;

  @property({attribute: 'logo-image-alternative'})
  logoImageAlternative?: string;

  @property({type: Boolean, attribute: 'username-is-email'})
  usernameIsEmail = false;

  @property({type: Boolean, attribute: 'allow-password-reset'})
  allowPasswordReset = false;

  @property({type: Boolean, attribute: 'allow-user-invite'})
  allowUserInvite = false;

  @property({attribute: 'return-url'})
  set returnPath(value: string) {
    umbAuthContext.returnPath = value;
  }

  /**
   * Override the default flow.
   */
  protected flow?: 'mfa' | 'reset-password' | 'invite-user';

  _form?: HTMLFormElement;
  _usernameLayoutItem?: UUIFormLayoutItemElement;
  _passwordLayoutItem?: UUIFormLayoutItemElement;
  _usernameInput?: UmbLoginInputElement;
  _passwordInput?: UmbLoginInputElement;
  _usernameLabel?: UUILabelElement;
  _passwordLabel?: UUILabelElement;

  constructor() {
    super();
    this.classList.add('uui-text');
    this.classList.add('uui-font');

    (this as unknown as EventTarget).addEventListener('umb-login-flow', (e) => {
      if (e instanceof CustomEvent) {
        this.flow = e.detail.flow || undefined;
      }
      this.requestUpdate();
    });
  }

  connectedCallback() {
    super.connectedCallback();

    this.#initializeForm();
  }

  disconnectedCallback() {
    super.disconnectedCallback();
    this._usernameLayoutItem?.remove();
    this._passwordLayoutItem?.remove();
    this._usernameLabel?.remove();
    this._usernameInput?.remove();
    this._passwordLabel?.remove();
    this._passwordInput?.remove();
  }

  /**
   * Creates the login form and adds it to the DOM in the default slot.
   * This is done to avoid having to deal with the shadow DOM, which is not supported in Google Chrome for autocomplete/autofill.
   *
   * @see Track this intent-to-ship for Chrome https://groups.google.com/a/chromium.org/g/blink-dev/c/RY9leYMu5hI?pli=1
   * @private
   */
  async #initializeForm() {
    const labelUsername = this.usernameIsEmail
      ? await umbLocalizationContext.localize('general_email', undefined, 'Email')
      : await umbLocalizationContext.localize('general_username', undefined, 'Username');
    const labelPassword = await umbLocalizationContext.localize('general_password', undefined, 'Password');
    const requiredMessage = await umbLocalizationContext.localize('general_required', undefined, 'Required');

    this._usernameInput = createInput({
      id: 'username-input',
      type: 'text',
      name: 'username',
      autocomplete: 'username',
      requiredMessage,
      label: labelUsername,
      inputmode: this.usernameIsEmail ? 'email' : '',
    });
    this._passwordInput = createInput({
      id: 'password-input',
      type: 'password',
      name: 'password',
      autocomplete: 'current-password',
      requiredMessage,
      label: labelPassword,
      inputmode: '',
    });
    this._usernameLabel = createLabel({
      forId: 'username-input',
      localizeAlias: this.usernameIsEmail ? 'general_email' : 'general_username',
    });
    this._passwordLabel = createLabel({forId: 'password-input', localizeAlias: 'general_password'});

    this._usernameLayoutItem = createFormLayoutItem(this._usernameLabel, this._usernameInput);
    this._passwordLayoutItem = createFormLayoutItem(this._passwordLabel, this._passwordInput);

    this._form = createForm([this._usernameLayoutItem, this._passwordLayoutItem]);

    this.insertAdjacentElement('beforeend', this._form);
  }

  render() {
    return html`
      <umb-auth-layout
        background-image=${ifDefined(this.backgroundImage)}
        logo-image=${ifDefined(this.logoImage)}
        logo-image-alternative=${ifDefined(this.logoImageAlternative)}>
        ${this._renderFlowAndStatus()}
      </umb-auth-layout>
    `;
  }

  private _renderFlowAndStatus() {
    const searchParams = new URLSearchParams(window.location.search);
    let flow = this.flow || searchParams.get('flow')?.toLowerCase();
    const status = searchParams.get('status');

    if (status === 'resetCodeExpired') {
      return html`
        <umb-error-layout
          header="Hi there"
          message=${until(
            umbLocalizationContext.localize(
              'login_resetCodeExpired',
              undefined,
              'The link you have clicked on is invalid or has expired'
            )
          )}>
        </umb-error-layout>`;
    }

    if (flow === 'invite-user' && status === 'false') {
      return html`
        <umb-error-layout
          header="Hi there"
          message=${until(
            umbLocalizationContext.localize(
              'user_userinviteExpiredMessage',
              undefined,
              'Welcome to Umbraco! Unfortunately your invite has expired. Please contact your administrator and ask them to resend it.'
            )
          )}>
        </umb-error-layout>`;
    }

    // validate
    if (flow) {
      if (flow === 'mfa' && !umbAuthContext.isMfaEnabled) {
        flow = undefined;
      }
    }

    switch (flow) {
      case 'mfa':
        return html`
          <umb-mfa-page></umb-mfa-page>`;
      case 'reset':
        return html`
          <umb-reset-password-page></umb-reset-password-page>`;
      case 'reset-password':
        return html`
          <umb-new-password-page></umb-new-password-page>`;
      case 'invite-user':
        return html`
          <umb-invite-page></umb-invite-page>`;

      default:
        return html`
          <umb-login-page
            ?allow-password-reset=${this.allowPasswordReset}
            ?username-is-email=${this.usernameIsEmail}>
            <slot></slot>
            <slot name="subheadline" slot="subheadline"></slot>
            <slot name="external" slot="external"></slot>
          </umb-login-page>`;
    }
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-auth': UmbAuthElement;
  }
}
