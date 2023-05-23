import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';

import './auth-layout.element';
import { UUIButtonState } from '@umbraco-ui/uui';
import { UmbAuthContext } from './types';
import { UmbAuthLegacyContext } from './auth-legacy.context';
import { UmbAuthNewContext } from './auth-new.context';

@customElement('umb-login')
export default class UmbLoginElement extends LitElement {
	#authContext: UmbAuthContext;

	@property({ type: String, attribute: 'return-url' })
	returnUrl = '';

	@property({ type: String, attribute: 'auth-url' })
	authUrl = '';

	@property({ type: Boolean })
	isLegacy = false;

	@state()
	private _loginState: UUIButtonState = undefined;

	@state()
	private _loginError = '';

	constructor() {
		super();
		if (this.isLegacy) {
			this.#authContext = new UmbAuthLegacyContext(this.authUrl, this.returnUrl);
		} else {
			this.#authContext = new UmbAuthNewContext(this.authUrl, this.returnUrl);
		}
	}

	#handleSubmit = async (e: SubmitEvent) => {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);

		const username = formData.get('email') as string;
		const password = formData.get('password') as string;
		const persist = formData.has('persist');

		this._loginState = 'waiting';

		const { error } = await this.#authContext.login({ username, password, persist });

		this._loginError = error || '';
		this._loginState = error ? 'failed' : 'success';
	};

	@state()
	private _greeting: string = this.#greeting;

	get #greeting() {
		return [
			'Happy super Sunday',
			'Happy marvelous Monday',
			'Happy tubular Tuesday',
			'Happy wonderful Wednesday',
			'Happy thunderous Thursday',
			'Happy funky Friday',
			'Happy Saturday',
		][new Date().getDay()];
	}

	render() {
		return html`
			<umb-auth-layout>
				<div class="uui-text">
					<h1 class="uui-h3">${this._greeting}</h1>
					<uui-form>
						<form id="LoginForm" name="login" @submit="${this.#handleSubmit}">
							<uui-form-layout-item>
								<uui-label id="emailLabel" for="email" slot="label" required>Email</uui-label>
								<uui-input
									type="email"
									id="email"
									name="email"
									required
									required-message="Email is required"></uui-input>
							</uui-form-layout-item>

							<uui-form-layout-item>
								<uui-label id="passwordLabel" for="password" slot="label" required>Password</uui-label>
								<uui-input-password
									id="password"
									name="password"
									required
									required-message="Password is required"></uui-input-password>
							</uui-form-layout-item>

							<uui-form-layout-item>
								<uui-checkbox name="persist" label="Remember me">Remember me</uui-checkbox>
							</uui-form-layout-item>

							<uui-form-layout-item>${this.#renderErrorMessage()}</uui-form-layout-item>

							<uui-button
								type="submit"
								label="Login"
								look="primary"
								color="positive"
								state=${this._loginState}></uui-button>
						</form>
					</uui-form>
				</div>
			</umb-auth-layout>
		`;
	}

	#renderErrorMessage() {
		if (!this._loginError || this._loginState !== 'failed') return;

		return html`<p class="text-danger">${this._loginError}</p>`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			#email,
			#password {
				width: 100%;
			}
			.text-danger {
				color: var(--uui-color-danger-standalone);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-login': UmbLoginElement;
	}
}
