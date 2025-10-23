import { html, customElement, property, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { InputType, UUIFormLayoutItemElement } from '@umbraco-cms/backoffice/external/uui';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

import { UMB_AUTH_CONTEXT, UmbAuthContext } from './contexts';
import { UmbSlimBackofficeController } from './controllers';

// We import the authStyles here so that we can inline it in the shadow DOM that is created outside of the UmbAuthElement.
import authStyles from './auth-styles.css?inline';

// Import the main bundle
import { extensions } from './umbraco-package.js';

const createInput = (opts: {
	id: string;
	type: InputType;
	name: string;
	autocomplete: AutoFill;
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
	input.autofocus = opts.autofocus || false;
	return input;
};

const createLabel = (opts: { forId: string; localizeAlias: string; localizeFallback: string }) => {
	const label = document.createElement('label');
	const umbLocalize: any = document.createElement('umb-localize');
	umbLocalize.key = opts.localizeAlias;
	umbLocalize.innerHTML = opts.localizeFallback;
	label.htmlFor = opts.forId;
	label.appendChild(umbLocalize);

	return label;
};

const createShowPasswordToggleButton = (opts: {
	id: string;
	name: string;
	ariaLabelShowPassword: string;
	ariaLabelHidePassword: string;
}) => {
	const openEyeSVG = `
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
			<path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"></path>
			<circle cx="12" cy="12" r="3"></circle>
		</svg>
    `;
	const closedEyeSVG = `
				<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">
					<path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"></path>
					<path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"></path>
					<path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61"></path>
					<line x1="2" x2="22" y1="2" y2="22"></line>
				</svg>
            `;

	const button = document.createElement('button');
	button.id = opts.id;
	button.ariaLabel = opts.ariaLabelShowPassword;
	button.name = opts.name;
	button.type = 'button';

	button.innerHTML = openEyeSVG;

	button.onclick = () => {
		const passwordInput = document.getElementById('password-input') as HTMLInputElement;

		if (passwordInput.type === 'password') {
			passwordInput.type = 'text';
			button.ariaLabel = opts.ariaLabelHidePassword;
			button.innerHTML = closedEyeSVG;
		} else {
			passwordInput.type = 'password';
			button.ariaLabel = opts.ariaLabelShowPassword;
			button.innerHTML = openEyeSVG;
		}

		passwordInput.focus();
	};

	return button;
};

const createShowPasswordToggleItem = (button: HTMLButtonElement) => {
	const span = document.createElement('span');
	span.id = 'password-show-toggle-span';
	span.appendChild(button);

	return span;
};

const createFormLayoutItem = (label: HTMLLabelElement, input: HTMLInputElement) => {
	const formLayoutItem = document.createElement('uui-form-layout-item') as UUIFormLayoutItemElement;

	formLayoutItem.appendChild(label);
	formLayoutItem.appendChild(input);

	return formLayoutItem;
};

const createFormLayoutPasswordItem = (
	label: HTMLLabelElement,
	input: HTMLInputElement,
	showPasswordToggle: HTMLSpanElement
) => {
	const formLayoutItem = document.createElement('uui-form-layout-item') as UUIFormLayoutItemElement;

	formLayoutItem.appendChild(label);
	const span = document.createElement('span');
	span.id = 'password-input-span';
	span.appendChild(input);
	span.appendChild(showPasswordToggle);
	formLayoutItem.appendChild(span);

	return formLayoutItem;
};

const createForm = (elements: HTMLElement[]) => {
	const styles = document.createElement('style');
	styles.innerHTML = authStyles;
	const form = document.createElement('form');
	form.id = 'umb-login-form';
	form.name = 'login-form';
	form.spellcheck = false;

	elements.push(styles);
	elements.forEach((element) => form.appendChild(element));

	return form;
};

@customElement('umb-auth')
export default class UmbAuthElement extends UmbLitElement {
	/**
	 * Disables the local login form and only allows external login providers.
	 *
	 * @attr disable-local-login
	 */
	@property({ type: Boolean, attribute: 'disable-local-login' })
	disableLocalLogin = false;

	@property({ attribute: 'background-image' })
	backgroundImage?: string;

	@property({ attribute: 'logo-image' })
	logoImage?: string;

	@property({ attribute: 'logo-image-alternative' })
	logoImageAlternative?: string;

	@property({ type: Boolean, attribute: 'username-is-email' })
	usernameIsEmail = false;

	@property({ type: Boolean, attribute: 'allow-password-reset' })
	allowPasswordReset = false;

	@property({ type: Boolean, attribute: 'allow-user-invite' })
	allowUserInvite = false;

	@property({ attribute: 'return-url' })
	set returnPath(value: string) {
		this.#authContext.returnPath = value;
	}
	get returnPath() {
		return this.#authContext.returnPath;
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
	_passwordShowPassswordToggleItem?: HTMLSpanElement;
	_passwordShowPasswordToggleButton?: HTMLButtonElement;

	#authContext = new UmbAuthContext(this, UMB_AUTH_CONTEXT);

	constructor() {
		super();

		(this as unknown as EventTarget).addEventListener('umb-login-flow', (e) => {
			if (e instanceof CustomEvent) {
				this.flow = e.detail.flow || undefined;
			}
			this.requestUpdate();
		});
	}

	async firstUpdated() {
		// Bind the (slim) Backoffice controller to this element so that we can use utilities from the Backoffice app.
		await new UmbSlimBackofficeController(this).register(this);

		// Register the main package for Umbraco.Auth
		umbExtensionsRegistry.registerMany(extensions);

		// Wait for localization to be ready before loading the form
		await this.#waitForLocalization();

		this.#initializeForm();
	}

	async #waitForLocalization(): Promise<void> {
		return new Promise((resolve) => {
			// Check if localization is already available
			if (this.localize.term('auth_showPassword') !== 'auth_showPassword') {
				resolve();
				return;
			}

			let retryCount = 0;
			const maxRetries = 40; //Retries 40 times with a 50ms interval = 2 seconds

			// If not, we check periodically until it it is available or we reach the max retries
			const checkInterval = setInterval(() => {
				if (this.localize.term('auth_showPassword') !== 'auth_showPassword' || retryCount >= maxRetries) {
					clearInterval(checkInterval);
					console.log('Localization is ready.');
					resolve();
				}
				retryCount++;
			}, 50);
		});
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this._usernameLayoutItem?.remove();
		this._passwordLayoutItem?.remove();
		this._usernameLabel?.remove();
		this._usernameInput?.remove();
		this._passwordLabel?.remove();
		this._passwordInput?.remove();
		this._passwordShowPassswordToggleItem?.remove();
		this._passwordShowPasswordToggleButton?.remove();
	}

	/**
	 * Creates the login form and adds it to the DOM in the default slot.
	 * This is done to avoid having to deal with the shadow DOM, which is not supported in Google Chrome for autocomplete/autofill.
	 *
	 * @see Track this intent-to-ship for Chrome https://groups.google.com/a/chromium.org/g/blink-dev/c/RY9leYMu5hI?pli=1
	 * @private
	 */
	#initializeForm() {
		this._usernameInput = createInput({
			id: 'username-input',
			type: 'text',
			name: 'username',
			autocomplete: 'username',
			inputmode: this.usernameIsEmail ? 'email' : '',
			autofocus: true,
		});
		this._passwordInput = createInput({
			id: 'password-input',
			type: 'password',
			name: 'password',
			autocomplete: 'current-password',
			inputmode: '',
		});
		this._passwordShowPasswordToggleButton = createShowPasswordToggleButton({
			id: 'password-show-toggle',
			name: 'password-show-toggle',
			ariaLabelShowPassword: this.localize.term('auth_showPassword'),
			ariaLabelHidePassword: this.localize.term('auth_hidePassword'),
		});
		this._passwordShowPassswordToggleItem = createShowPasswordToggleItem(this._passwordShowPasswordToggleButton);
		this._usernameLabel = createLabel({
			forId: 'username-input',
			localizeAlias: this.usernameIsEmail ? 'auth_email' : 'auth_username',
			localizeFallback: this.usernameIsEmail ? 'Email' : 'Username',
		});
		this._passwordLabel = createLabel({
			forId: 'password-input',
			localizeAlias: 'auth_password',
			localizeFallback: 'Password',
		});
		this._usernameLayoutItem = createFormLayoutItem(this._usernameLabel, this._usernameInput);
		this._passwordLayoutItem = createFormLayoutPasswordItem(
			this._passwordLabel,
			this._passwordInput,
			this._passwordShowPassswordToggleItem
		);

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
		if (this.disableLocalLogin) {
			return html`
				<umb-error-layout no-back-link>
					<umb-localize key="auth_localLoginDisabled"
						>Unfortunately, it is not possible to log in directly. It has been disabled by a login
						provider.</umb-localize
					>
				</umb-error-layout>
			`;
		}

		const searchParams = new URLSearchParams(window.location.search);
		let flow = this.flow || searchParams.get('flow')?.toLowerCase();
		const status = searchParams.get('status');

		if (status === 'resetCodeExpired') {
			return html` <umb-error-layout message=${this.localize.term('auth_resetCodeExpired')}> </umb-error-layout>`;
		}

		if (flow === 'invite-user' && status === 'false') {
			return html` <umb-error-layout message=${this.localize.term('auth_userInviteExpiredMessage')}>
			</umb-error-layout>`;
		}

		// validate
		if (flow) {
			if (flow === 'mfa' && !this.#authContext.isMfaEnabled) {
				flow = undefined;
			}
		}

		switch (flow) {
			case 'mfa':
				return html` <umb-mfa-page></umb-mfa-page>`;
			case 'reset':
				return html` <umb-reset-password-page></umb-reset-password-page>`;
			case 'reset-password':
				return html` <umb-new-password-page></umb-new-password-page>`;
			case 'invite-user':
				return html` <umb-invite-page></umb-invite-page>`;

			default:
				return html` <umb-login-page
					?allow-password-reset=${this.allowPasswordReset}
					?username-is-email=${this.usernameIsEmail}>
					<slot></slot>
					<slot name="subheadline" slot="subheadline"></slot>
				</umb-login-page>`;
		}
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth': UmbAuthElement;
	}
}
