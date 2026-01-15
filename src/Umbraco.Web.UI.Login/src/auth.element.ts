import { html, customElement, property, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { InputType, UUIFormLayoutItemElement } from '@umbraco-cms/backoffice/external/uui';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

import { UMB_AUTH_CONTEXT, UmbAuthContext } from './contexts/index.js';
import { UmbSlimBackofficeController } from './controllers/index.js';

// We import the authStyles here so that we can inline it in the shadow DOM that is created outside of the UmbAuthElement.
import authStyles from './auth-styles.css?inline';

// Import the SVG files
import svgEyeOpen from './assets/eye-open.svg?raw';
import svgEyeClosed from './assets/eye-closed.svg?raw';

// Import the main bundle
import { extensions } from './umbraco-package.js';

const createInput = (opts: {
	id: string;
	type: InputType;
	name: string;
	autocomplete: AutoFill;
	errorId: string;
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
	input.setAttribute('aria-errormessage', opts.errorId);
	input.autofocus = opts.autofocus || false;
	input.className = 'input';

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

const createValidationMessage = (errorId: string) => {
	const validationElement = document.createElement('div');
	validationElement.className = 'errormessage';
	validationElement.id = errorId;
	validationElement.role = 'alert';
	return validationElement;
};

const createShowPasswordToggleButton = (opts: {
	id: string;
	name: string;
	ariaLabelShowPassword: string;
	ariaLabelHidePassword: string;
}) => {
	const button = document.createElement('button');
	button.id = opts.id;
	button.ariaLabel = opts.ariaLabelShowPassword;
	button.name = opts.name;
	button.type = 'button';

	button.innerHTML = svgEyeOpen;

	button.onclick = () => {
		const passwordInput = document.getElementById('password-input') as HTMLInputElement;

		if (passwordInput.type === 'password') {
			passwordInput.type = 'text';
			button.ariaLabel = opts.ariaLabelHidePassword;
			button.innerHTML = svgEyeClosed;
		} else {
			passwordInput.type = 'password';
			button.ariaLabel = opts.ariaLabelShowPassword;
			button.innerHTML = svgEyeOpen;
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

const createFormLayoutItem = (label: HTMLLabelElement, input: HTMLInputElement, localizationKey: string) => {
	const formLayoutItem = document.createElement('uui-form-layout-item') as UUIFormLayoutItemElement;
	const errorId = input.getAttribute('aria-errormessage') || input.id + '-error';

	formLayoutItem.appendChild(label);
	formLayoutItem.appendChild(input);

	const validationMessage = createValidationMessage(errorId);
	formLayoutItem.appendChild(validationMessage);

	// Bind validation
	input.oninput = () => validateInput(input, validationMessage, localizationKey);
	input.onblur = () => validateInput(input, validationMessage, localizationKey);
	input.oninvalid = () => validateInput(input, validationMessage, localizationKey);

	return formLayoutItem;
};

const createFormLayoutPasswordItem = (
	label: HTMLLabelElement,
	input: HTMLInputElement,
	showPasswordToggle: HTMLSpanElement,
	requiredMessageKey: string
) => {
	const formLayoutItem = document.createElement('uui-form-layout-item') as UUIFormLayoutItemElement;
	const errorId = input.getAttribute('aria-errormessage') || input.id + '-error';

	formLayoutItem.appendChild(label);

	const span = document.createElement('span');
	span.id = 'password-input-span';
	span.appendChild(input);
	span.appendChild(showPasswordToggle);
	formLayoutItem.appendChild(span);

	const validationMessage = createValidationMessage(errorId);
	formLayoutItem.appendChild(validationMessage);

	// Bind validation
	input.oninput = () => validateInput(input, validationMessage, requiredMessageKey);
	input.onblur = () => validateInput(input, validationMessage, requiredMessageKey);
	input.oninvalid = () => validateInput(input, validationMessage, requiredMessageKey);

	return formLayoutItem;
};

const validateInput = (input: HTMLInputElement, validationElement: HTMLElement, requiredMessage = '') => {
	validationElement.innerHTML = '';
	if (input.validity.valid) {
		input.removeAttribute('aria-invalid');
		validationElement.classList.remove('active');
		validationElement.ariaLive = 'off';
	} else {
		input.setAttribute('aria-invalid', 'true');

		const localizeElement = document.createElement('umb-localize');
		localizeElement.innerHTML = input.validationMessage;
		localizeElement.key = requiredMessage;
		validationElement.appendChild(localizeElement);

		validationElement.classList.add('active');
		validationElement.ariaLive = 'assertive';
	}
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

	#authContext = new UmbAuthContext(this, UMB_AUTH_CONTEXT);

	constructor() {
		super();

		(this as unknown as EventTarget).addEventListener('umb-login-flow', (e) => {
			if (e instanceof CustomEvent) {
				this.flow = e.detail.flow || undefined;
				if (typeof e.detail.status !== 'undefined') {
					const searchParams = new URLSearchParams(window.location.search);
					if (e.detail.status === null) {
						searchParams.delete('status');
					} else {
						searchParams.set('status', e.detail.status);
					}
					const newRelativePathQuery = window.location.pathname + '?' + searchParams.toString();
					window.history.pushState(null, '', newRelativePathQuery);
				}
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
		return new Promise((resolve, reject) => {
			let retryCount = 0;
			// Retries 40 times with a 50ms interval = 2 seconds
			const maxRetries = 40;

			// We check periodically until it is available or we reach the max retries
			const checkInterval = setInterval(() => {
				// If we reach max retries, we give up and reject the promise
				if (retryCount > maxRetries) {
					clearInterval(checkInterval);
					reject('Localization not available');
					return;
				}
				// Check if localization is available
				if (this.localize.term('auth_showPassword') !== 'auth_showPassword') {
					clearInterval(checkInterval);
					resolve();
					return;
				}
				retryCount++;
			}, 50);
		});
	}

	/**
	 * Creates the login form and adds it to the DOM in the default slot.
	 * This is done to avoid having to deal with the shadow DOM, which is not supported in Google Chrome for autocomplete/autofill.
	 *
	 * @see Track this intent-to-ship for Chrome https://groups.google.com/a/chromium.org/g/blink-dev/c/RY9leYMu5hI?pli=1
	 * @private
	 */
	#initializeForm() {
		const usernameInput = createInput({
			id: 'username-input',
			type: 'text',
			name: 'username',
			autocomplete: 'username',
			errorId: 'username-input-error',
			inputmode: this.usernameIsEmail ? 'email' : '',
			autofocus: true,
		});
		const passwordInput = createInput({
			id: 'password-input',
			type: 'password',
			name: 'password',
			autocomplete: 'current-password',
			errorId: 'password-input-error',
			inputmode: '',
		});
		const passwordShowPasswordToggleButton = createShowPasswordToggleButton({
			id: 'password-show-toggle',
			name: 'password-show-toggle',
			ariaLabelShowPassword: this.localize.term('auth_showPassword'),
			ariaLabelHidePassword: this.localize.term('auth_hidePassword'),
		});
		const passwordShowPasswordToggleItem = createShowPasswordToggleItem(passwordShowPasswordToggleButton);
		const usernameLabel = createLabel({
			forId: 'username-input',
			localizeAlias: this.usernameIsEmail ? 'auth_email' : 'auth_username',
			localizeFallback: this.usernameIsEmail ? 'Email' : 'Username',
		});
		const passwordLabel = createLabel({
			forId: 'password-input',
			localizeAlias: 'auth_password',
			localizeFallback: 'Password',
		});
		const usernameLayoutItem = createFormLayoutItem(
			usernameLabel,
			usernameInput,
			this.usernameIsEmail ? 'auth_requiredEmailValidationMessage' : 'auth_requiredUsernameValidationMessage'
		);
		const passwordLayoutItem = createFormLayoutPasswordItem(
			passwordLabel,
			passwordInput,
			passwordShowPasswordToggleItem,
			'auth_requiredPasswordValidationMessage'
		);
		const style = document.createElement('style');
		style.innerHTML = authStyles;
		document.head.appendChild(style);

		const form = document.createElement('form');
		form.id = 'umb-login-form';
		form.name = 'login-form';
		form.spellcheck = false;
		form.setAttribute('novalidate', '');

		form.appendChild(usernameLayoutItem);
		form.appendChild(passwordLayoutItem);

		this.insertAdjacentElement('beforeend', form);
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
				return html`
					<umb-login-page ?allow-password-reset=${this.allowPasswordReset} ?username-is-email=${this.usernameIsEmail}>
						<slot></slot>
					</umb-login-page>
				`;
		}
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth': UmbAuthElement;
	}
}
