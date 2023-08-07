import { UUIButtonState, UUIInputElement, UUITextStyles } from '@umbraco-ui/uui';
import { LitElement, css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { until } from 'lit/directives/until.js';
import { UmbAuthMainContext } from '../context/auth-main.context';
import { umbLocalizationContext } from '../localization/localization-context';

@customElement('umb-mfa-page')
export default class UmbMfaPageElement extends LitElement {
	@state()
	protected providers: Option[] = [];

	@state()
	private loading = true;

	@state()
	private buttonState?: UUIButtonState;

	@state()
	private error: string | null = null;

	constructor() {
		super();
		this.#loadProviders();
	}

	async #loadProviders() {
		try {
			const response = await UmbAuthMainContext.Instance.getMfaProviders();
			this.providers = response.providers.map((provider) => ({ name: provider, value: provider }));

			if (this.providers.length) {
				this.providers[0].selected = true;
			}

			if (response.error) {
				this.error = response.error;
			}
		} catch (e) {
			if (e instanceof Error) {
				this.error = e.message ?? 'Unknown error';
			} else {
				this.error = 'Unknown error';
			}
			this.providers = [];
		}
		this.loading = false;
	}

	private async handleSubmit(e: SubmitEvent) {
		e.preventDefault();

		this.error = null;

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const codeInput = form.elements.namedItem('2facode') as UUIInputElement;

		if (codeInput) {
			codeInput.error = false;
			codeInput.errorMessage = '';
		}

		if (!form.checkValidity()) return;

		const formData = new FormData(form);

		let provider = formData.get('provider') as string;

		// If no provider given, use the first one (there probably is only one anyway)
		if (!provider) {
			provider = this.providers[0].value;
		}

		if (!provider) {
			this.error = 'No provider selected';
			return;
		}

		const code = formData.get('token') as string;

		this.buttonState = 'waiting';

		try {
			const response = await UmbAuthMainContext.Instance.validateMfaCode(code, provider);
			if (response.error) {
				if (codeInput) {
					codeInput.error = true;
					codeInput.errorMessage = response.error;
				} else {
					this.error = response.error;
				}
				this.buttonState = 'failed';
				return;
			}

			this.buttonState = 'success';

			const returnPath = UmbAuthMainContext.Instance.returnPath;
			if (returnPath) {
				location.href = returnPath;
			}

			this.dispatchEvent(new CustomEvent('login-success', { bubbles: true, composed: true }));
		} catch (e) {
			if (e instanceof Error) {
				this.error = e.message ?? 'Unknown error';
			} else {
				this.error = 'Unknown error';
			}
			this.buttonState = 'failed';
		}
	}

	renderProviderStep() {
		return html`
			<uui-form>
				<form id="LoginForm" @submit=${this.handleSubmit}>
					<header>
						<h1>
							<umb-localize key="login_2fatitle">One last step</umb-localize>
						</h1>
					</header>

					<p>
						<umb-localize key="login_2faText">
							You have enabled 2-factor authentication and must verify your identity.
						</umb-localize>
					</p>

					<!-- if there's only one provider active, it will skip this step! -->
					${this.providers.length > 1
						? html`
								<uui-form-layout-item label="@login_2faMultipleText">
									<uui-label id="providerLabel" for="provider" slot="label" required>
										<umb-localize key="login_2faMultipleText">Please choose a 2-factor provider</umb-localize>
									</uui-label>
									<uui-select id="provider" name="provider" .options=${this.providers} aria-required="true" required>
									</uui-select>
								</uui-form-layout-item>
						  `
						: nothing}

					<uui-form-layout-item>
						<uui-label id="2facodeLabel" for="2facode" slot="label" required>
							<umb-localize key="login_2faCodeInput">Verification code</umb-localize>
						</uui-label>

						<uui-input
							autofocus
							id="2facode"
							type="text"
							name="token"
							inputmode="numeric"
							autocomplete="one-time-code"
							placeholder=${until(
								umbLocalizationContext.localize('login_2faCodeInputHelp', 'Please enter the verification code')
							)}
							aria-required="true"
							required
							required-message=${until(
								umbLocalizationContext.localize('login_2faCodeInputHelp', 'Please enter the verification code')
							)}
							style="width:100%;">
						</uui-input>
					</uui-form-layout-item>

					<div id="actions">
						<uui-button
							.state=${this.buttonState}
							button-style="success"
							look="primary"
							color="default"
							label=${until(umbLocalizationContext.localize('general_validate', 'Validate'))}
							type="submit"></uui-button>
						<uui-button
							label=${until(umbLocalizationContext.localize('general_back', 'Back'))}
							href="login"></uui-button>
					</div>
					${this.error ? html` <span class="text-danger">${this.error}</span> ` : nothing}
				</form>
			</uui-form>
		`;
	}

	render() {
		return this.loading ? html`<uui-loader-bar></uui-loader-bar>` : this.renderProviderStep();
	}

	static styles = [
		UUITextStyles,
		css`
			.text-danger {
				color: var(--uui-color-danger-standalone);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-mfa-page': UmbMfaPageElement;
	}
}
