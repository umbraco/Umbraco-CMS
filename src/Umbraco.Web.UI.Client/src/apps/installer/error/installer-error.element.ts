import { css, CSSResultGroup, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbInstallerContext, UMB_INSTALLER_CONTEXT_TOKEN } from '../installer.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-installer-error')
export class UmbInstallerErrorElement extends UmbLitElement {
	@state()
	_error?: ProblemDetailsModel;

	private _installerContext?: UmbInstallerContext;

	connectedCallback() {
		super.connectedCallback();

		this.consumeContext(UMB_INSTALLER_CONTEXT_TOKEN, (installerContext) => {
			this._installerContext = installerContext;
			this._observeInstallStatus();
		});
	}

	private _observeInstallStatus() {
		if (!this._installerContext) return;

		this.observe(this._installerContext.installStatusChanges(), (status) => {
			if (status) {
				this._error = status;
			}
		});
	}

	private _handleSubmit(e: SubmitEvent) {
		e.preventDefault();
		this._installerContext?.reset();
	}

	private _renderError(error: ProblemDetailsModel) {
		return html`
			<p>Description: ${error.title}</p>
			${error.errors ? this._renderErrors(error.errors) : nothing}
			<hr />
			<h3>Details:</h3>
			<p id="error-message" data-test="error-message">${error.detail ?? 'Unknown error'}</p>
		`;
	}

	private _renderErrors(errors: Record<string, unknown>) {
		return html`
			<h3>Errors:</h3>
			<ul>
				${Object.keys(errors).map((key) => html` <li>${key}: ${(errors[key] as string[]).join(', ')}</li> `)}
			</ul>
		`;
	}

	render() {
		return html` <div id="container" class="uui-text" data-test="installer-error">
			<uui-form>
				<form id="installer-form" @submit="${this._handleSubmit}">
					<h2>Something went wrong:</h2>
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
				color: var(--uui-color-danger, #d42054);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer-error': UmbInstallerErrorElement;
	}
}
