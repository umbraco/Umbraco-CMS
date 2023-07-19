import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';

import type { UUIButtonState } from '@umbraco-ui/uui';
import { UmbAuthMainContext } from './context/auth-main.context.js';

import './auth-layout.element.js';
import './reset-password.element.js';
import './new-password.element.js';

@customElement('umb-login')
export default class UmbLoginElement extends LitElement {
	#authContext = UmbAuthMainContext.Instance;

	@state()
	private _loginState: UUIButtonState = undefined;

	@state()
	private _loginError = '';

	@state()
	private _allowPasswordReset = true; // GET FROM CONTEXT

	#handleSubmit = async (e: SubmitEvent) => {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		if (!form.checkValidity()) return;

		const formData = new FormData(form);

		const username = formData.get('email') as string;
		const password = formData.get('password') as string;
		const persist = formData.has('persist');

		this._loginState = 'waiting';

		const response = await this.#authContext.login({
			username,
			password,
			persist,
		});

		this._loginError = response.error || '';
		this._loginState = response.error ? 'failed' : 'success';

		if (response.error) return;

		const returnPath = this.#authContext.returnPath;

		if (returnPath) {
			location.href = returnPath;
		}

		this.dispatchEvent(new CustomEvent('login-success', { bubbles: true, composed: true }));
	};

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
			<h1 class="uui-h3">${this.#greeting}</h1>
			<uui-form>
				<form id="LoginForm" name="login" @submit="${this.#handleSubmit}">
					<uui-form-layout-item>
						<uui-label id="emailLabel" for="email" slot="label" required>Email</uui-label>
						<uui-input
							type="email"
							id="email"
							name="email"
							label="Email"
							required
							required-message="Email is required"></uui-input>
					</uui-form-layout-item>

					<uui-form-layout-item>
						<uui-label id="passwordLabel" for="password" slot="label" required>Password</uui-label>
						<uui-input-password
							id="password"
							name="password"
							label="Password"
							required
							required-message="Password is required"></uui-input-password>
					</uui-form-layout-item>

					<div id="secondary-actions">
						${when(
							this.#authContext.supportsPersistLogin,
							() => html`<uui-form-layout-item>
								<uui-checkbox name="persist" label="Remember me">Remember me</uui-checkbox>
							</uui-form-layout-item>`
						)}
						${when(this._allowPasswordReset, () => html`<a href="reset"> Forgot password? </a>`)}
					</div>

					<uui-form-layout-item>${this.#renderErrorMessage()}</uui-form-layout-item>

					<uui-button
						type="submit"
						id="login-button"
						label="Login"
						look="primary"
						color="positive"
						.state=${this._loginState}></uui-button>
				</form>
			</uui-form>
		`;
	}

	#renderErrorMessage() {
		if (!this._loginError || this._loginState !== 'failed') return nothing;

		return html`<p class="text-danger">${this._loginError}</p>`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-input,
			uui-input-password {
				width: 100%;
			}
			#login-button {
				margin-left: auto;
				display: flex;
				width: fit-content;
			}
			.text-danger {
				color: var(--uui-color-danger-standalone);
			}
			#secondary-actions {
				display: flex;
				align-items: center;
				justify-content: space-between;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-login': UmbLoginElement;
	}
}
