import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';

import type { UUIButtonState } from '@umbraco-ui/uui';
import type { IUmbAuthContext } from './types.js';
import { UmbAuthMainContext } from './context/auth-main.context.js';

import './auth-layout.element.js';
import './reset-password.element.js';
import './new-password.element.js';

@customElement('umb-login')
export default class UmbLoginElement extends LitElement {
	#authContext!: IUmbAuthContext;
	#returnUrl = '';

	pages = ['login', 'reset', 'new'] as const;

	@state()
	private page: (typeof this.pages)[number] = 'login';

	@property({ type: String, attribute: 'return-url' })
	set returnUrl(value: string) {
		this.#returnUrl = value;
	}

	get returnUrl() {
		// Check if there is a ?redir querystring or else return the returnUrl attribute
		return new URLSearchParams(window.location.search).get('redir') || this.#returnUrl;
	}

	@property({ type: Boolean, attribute: 'is-legacy' })
	isLegacy = false;

	@property({ type: Boolean, attribute: 'allow-password-reset' })
	allowPasswordReset = true;

	@state()
	private _loginState: UUIButtonState = undefined;

	@state()
	private _loginError = '';

	connectedCallback(): void {
		super.connectedCallback();

		new UmbAuthMainContext(this.isLegacy);
		this.#authContext = UmbAuthMainContext.Instance;

		window.addEventListener('popstate', this.#handleUrlChange);

		// Save a reference to the original `pushState` method
		const originalPushState = history.pushState;

		// Override `pushState` with a custom implementation
		history.pushState = function (state, title, url) {
			// Call the original `pushState` method
			//@ts-ignore
			originalPushState.apply(history, arguments);
			// Dispatch a custom event
			window.dispatchEvent(new CustomEvent('popstate', { detail: { state, title, url } }));
		};

		this.#setPage(window.location.pathname);
	}

	#setPage(url: any) {
		this.page = this.pages.includes(url) ? url : this.pages[0];
	}

	#handleUrlChange = () => {
		const extractPage = document.location.pathname.replace(/^\/(.*)/, '$1');
		this.#setPage(extractPage);
	};

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

		if (this.returnUrl) {
			location.href = this.returnUrl;
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
			<div
				style="background: white; padding: 16px; position: absolute; left: 50%; top: 100px; z-index: 1; transform: translateX(-50%);">
				<label for="page">DEBUG set page</label>
				<select
					style="padding: 6px; font-size: 20px;"
					id="page"
					name="page"
					@change=${(e: any) => history.pushState({}, '', e.target.value)}>
					${this.pages.map((page) => html`<option value="${page}" ?selected="${this.page === page}">${page}</option>`)}
				</select>
			</div>
			<umb-auth-layout>${this.#renderRoute()}</umb-auth-layout>
		`;
	}

	#renderRoute() {
		switch (this.page) {
			case 'login':
				return this.#renderLoginPage();

			case 'reset':
				return html`<umb-reset-password></umb-reset-password>`;

			case 'new':
				return html`<umb-new-password></umb-new-password>`;
		}
	}
	#renderLoginPage() {
		return html`
			<div class="uui-text">
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
							<!-- TODO: Should this be a link instead? -->
							${when(
								this.allowPasswordReset,
								() =>
									html`
										<uui-button style="height: min-content" @click=${() => (this.page = 'reset')}>
											Forgot password?
										</uui-button>
									`
							)}
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
			</div>
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
