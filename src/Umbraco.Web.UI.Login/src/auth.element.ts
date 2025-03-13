import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { until } from 'lit/directives/until.js';

import { umbAuthContext } from './context/auth.context.js';
import { umbLocalizationContext } from './external/localization/localization-context.js';
import type { InputType, UUIFormLayoutItemElement } from '@umbraco-ui/uui';

import authStyles from './auth-styles.css?inline';

const createInput = (opts: {
  id: string;
  type: InputType;
  name: string;
  autocomplete: AutoFill;
  label: string;
  inputmode: string;
  autofocus?: boolean;
}) => {
  const input = document.createElement('input');
  input.type = opts.type;
  input.name = opts.name;
  input.autocomplete = opts.autocomplete;
  input.id = opts.id;
  input.required = true;
  input.inputMode = opts.inputmode;
  input.ariaLabel = opts.label;
  input.autofocus = opts.autofocus || false;

  return input;
};

const createLabel = (opts: { forId: string; localizeAlias: string; localizeFallback: string; }) => {
  const label = document.createElement('label');
  const umbLocalize: any = document.createElement('umb-localize');
  umbLocalize.key = opts.localizeAlias;
  umbLocalize.innerHTML = opts.localizeFallback;
  label.htmlFor = opts.forId;
  label.appendChild(umbLocalize);

  return label;
};

const createFormLayoutItem = (label: HTMLLabelElement, input: HTMLInputElement) => {
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
  get disableLocalLogin() {
    return umbAuthContext.disableLocalLogin;
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
  get returnPath() {
    return umbAuthContext.returnPath;
  }

  /**
   * Override the default flow.
   */
  protected flow?: 'mfa' | 'reset-password' | 'invite-user';

  _form?: HTMLFormElement;
  _usernameLayoutItem?: UUIFormLayoutItemElement;
  _passwordLayoutItem?: UUIFormLayoutItemElement;
  _usernameInput?: HTMLInputElement;
  _passwordInput?: HTMLInputElement;
  _usernameLabel?: HTMLLabelElement;
  _passwordLabel?: HTMLLabelElement;

  constructor() {
    super();

    (this as unknown as EventTarget).addEventListener('umb-login-flow', (e) => {
      if (e instanceof CustomEvent) {
        this.flow = e.detail.flow || undefined;
      }
      this.requestUpdate();
    });
  }

  connectedCallback() {
    super.connectedCallback();
    this.classList.add('uui-text');
    this.classList.add('uui-font');

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

    this._usernameInput = createInput({
      id: 'username-input',
      type: 'text',
      name: 'username',
      autocomplete: 'username',
      label: labelUsername,
      inputmode: this.usernameIsEmail ? 'email' : '',
      autofocus: true,
    });
    this._passwordInput = createInput({
      id: 'password-input',
      type: 'password',
      name: 'password',
      autocomplete: 'current-password',
      label: labelPassword,
      inputmode: '',
    });
    this._usernameLabel = createLabel({
      forId: 'username-input',
      localizeAlias: this.usernameIsEmail ? 'general_email' : 'general_username',
      localizeFallback: this.usernameIsEmail ? 'Email' : 'Username',
    });
    this._passwordLabel = createLabel({forId: 'password-input', localizeAlias: 'general_password', localizeFallback: 'Password'});

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
