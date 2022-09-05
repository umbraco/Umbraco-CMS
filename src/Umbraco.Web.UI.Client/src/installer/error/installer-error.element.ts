import { css, CSSResultGroup, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../core/context';

import type { ProblemDetails } from '../../core/models';
import { UmbInstallerContext } from '../installer.context';

@customElement('umb-installer-error')
export class UmbInstallerErrorElement extends UmbContextConsumerMixin(LitElement) {
	static styles: CSSResultGroup = [
		css`
			:host,
			#container {
				display: flex;
				flex-direction: column;
				height: 100%;
			}

			h1 {
				text-align: center;
			}

			#error-message {
				color: var(--uui-color-error, red);
			}
		`,
	];

	@state()
	_error?: ProblemDetails;

	private _installerContext?: UmbInstallerContext;
	private _installStatusSubscription?: Subscription;

	connectedCallback() {
		super.connectedCallback();

		this.consumeContext('umbInstallerContext', (installerContext) => {
			this._installerContext = installerContext;
			this._observeInstallStatus();
		});
	}

	private _observeInstallStatus() {
		this._installStatusSubscription?.unsubscribe();

		this._installerContext?.installStatusChanges().subscribe((status) => {
			if (status) {
				this._error = status;
			}
		});
	}

	private _handleSubmit(e: SubmitEvent) {
		e.preventDefault();
		this._installerContext?.reset();
	}

	private _renderError(error: ProblemDetails) {
		return html`
			<p id="error-message" data-test="error-message">${error.detail ?? 'Unknown error'}</p>
			<hr />
			${error.errors ? this._renderErrors(error.errors) : nothing}
		`;
	}

	private _renderErrors(errors: Record<string, unknown>) {
		return html`
			<ul>
				${Object.keys(errors).map((key) => html` <li>${key}: ${(errors[key] as string[]).join(', ')}</li> `)}
			</ul>
		`;
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._installStatusSubscription?.unsubscribe();
	}

	render() {
		return html` <div id="container" class="uui-text" data-test="installer-error">
			<uui-form>
				<form id="installer-form" @submit="${this._handleSubmit}">
					<h1 class="uui-h3">Installing Umbraco</h1>
					<h2>Something went wrong</h2>
					${this._error ? this._renderError(this._error) : nothing}
					<div id="buttons">
						<uui-button
							id="button-reset"
							type="submit"
							label="Go back and try again"
							look="primary"
							color="positive"></uui-button>
					</div>
				</form>
			</uui-form>
		</div>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer-error': UmbInstallerErrorElement;
	}
}
