import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { until } from 'lit/directives/until.js';

import { umbAuthContext } from './context/auth.context.js';
import { umbLocalizationContext } from './external/localization/localization-context.js';
import { UmbLocalizeElement } from './external/localization/localize.element.js';
import { UMBLoginInputElement } from './components/logIn-input.element.js';
import { InputType, UUIFormLayoutItemElement, UUILabelElement } from '@umbraco-ui/uui';

import authStyles from './auth-styles.css?inline';

const createInput = (id: string, type: InputType, name: string, autocomplete: AutoFill, requiredMessage: string, label: string) => {
	const input = document.createElement('umb-login-input');
	input.type = type;
	input.name = name;
	input.autocomplete = autocomplete;
	input.id = id;
	input.required = true;
	input.requiredMessage = requiredMessage;
  input.label = label;

	return input;
};

const createLabel = (forId: string, localizeAlias: string) => {
	const label = document.createElement('uui-label');
	const umbLocalize = document.createElement('umb-localize') as UmbLocalizeElement;
	umbLocalize.key = localizeAlias;
	label.for = forId;
	label.appendChild(umbLocalize);

	return label;
};

const createFormLayoutItem = (label: UUILabelElement, input: UMBLoginInputElement) => {
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
	const submitButton = document.createElement('input');
	submitButton.type = 'submit';
	submitButton.ariaHidden = 'true';
	submitButton.style.display = 'none';

	elements.push(styles);
	elements.push(submitButton);
	elements.forEach((element) => form.appendChild(element));

	return form;
};

@customElement('umb-auth')
export default class UmbAuthElement extends LitElement {
	#returnPath = '';

	/**
	 * Disables the local login form and only allows external login providers.
	 *
	 * @attr disable-local-login
	 */
	@property({ type: Boolean, attribute: 'disable-local-login' })
	set disableLocalLogin(value: boolean) {
		umbAuthContext.disableLocalLogin = value;
	}

	@property({ attribute: 'background-image' })
	backgroundImage?: string;

	@property({ attribute: 'logo-light' })
	logoLight?: string;

	@property({ attribute: 'logo-dark' })
	logoDark?: string;

	@property({ type: Boolean, attribute: 'username-is-email' })
	usernameIsEmail = false;

	@property({ type: Boolean, attribute: 'allow-password-reset' })
	allowPasswordReset = false;

	@property({ type: Boolean, attribute: 'allow-user-invite' })
	allowUserInvite = false;

	@property({ type: String, attribute: 'return-url' })
	set returnPath(value: string) {
		this.#returnPath = value;
		umbAuthContext.returnPath = this.returnPath;
	}
	get returnPath() {
		// Check if there is a ?redir querystring or else return the returnUrl attribute
		return new URLSearchParams(window.location.search).get('returnPath') || this.#returnPath;
	}

	/**
	 * Override the default flow.
	 */
	protected flow?: 'mfa' | 'reset-password' | 'invite-user';

	//TODO We could probably just save the form. everything inside should be cleaned up when it's removed
	_form?: HTMLFormElement;
	_usernameLayoutItem?: UUIFormLayoutItemElement;
	_passwordLayoutItem?: UUIFormLayoutItemElement;
	_usernameInput?: UMBLoginInputElement;
	_passwordInput?: UMBLoginInputElement;
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
    const labelUsername =
      this.usernameIsEmail
        ? await umbLocalizationContext.localize('general_username', undefined, 'Username')
        : await umbLocalizationContext.localize('general_email', undefined, 'Email');
    const labelPassword = await umbLocalizationContext.localize('general_password', undefined, 'Password');
		const requiredMessage = await umbLocalizationContext.localize('general_required', undefined, 'Required');

		this._usernameInput = createInput(
			'username-input',
			this.usernameIsEmail ? 'email' : 'text',
			'username',
			'username',
			requiredMessage,
      labelUsername
		);
		this._passwordInput = createInput('password-input', 'password', 'password', 'current-password', requiredMessage, labelPassword);
		this._usernameLabel = createLabel('username-input', this.usernameIsEmail ? 'general_email' : 'user_username');
		this._passwordLabel = createLabel('password-input', 'user_password');

		this._usernameLayoutItem = createFormLayoutItem(this._usernameLabel, this._usernameInput);
		this._passwordLayoutItem = createFormLayoutItem(this._passwordLabel, this._passwordInput);

		this._form = createForm([this._usernameLayoutItem, this._passwordLayoutItem]);

		this.insertAdjacentElement('beforeend', this._form);
	}

	render() {
		return html`
			<umb-auth-layout
				background-image=${ifDefined(this.backgroundImage)}
				logo-light=${ifDefined(this.logoLight)}
				logo-dark=${ifDefined(this.logoDark)}>
				${this._renderFlowAndStatus()}
			</umb-auth-layout>
		`;
	}

	private _renderFlowAndStatus() {
		const searchParams = new URLSearchParams(window.location.search);
		let flow = this.flow || searchParams.get('flow')?.toLowerCase();
		const status = searchParams.get('status');

		if (status === 'resetCodeExpired') {
			return html` <umb-error-layout
				header="Hi there"
				message=${until(
					umbLocalizationContext.localize('login_resetCodeExpired', undefined, 'The link you have clicked on is invalid or has expired')
				)}>
			</umb-error-layout>`;
		}

		if (flow === 'invite-user' && status === 'false') {
			return html` <umb-error-layout
				header="Hi there"
				message=${until(
					umbLocalizationContext.localize('user_userinviteExpiredMessage', undefined, 'Welcome to Umbraco! Unfortunately your invite has expired. Please contact your administrator and ask them to resend it.'),
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
				return html`<umb-mfa-page></umb-mfa-page>`;
			case 'reset':
				return html`<umb-reset-password-page></umb-reset-password-page>`;
			case 'reset-password':
				return html`<umb-new-password-page></umb-new-password-page>`;
			case 'invite-user':
				return html`<umb-invite-page></umb-invite-page>`;

			default:
				return html`<umb-login-page
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
