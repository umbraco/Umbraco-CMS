import { css, CSSResultGroup, html, LitElement, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import type { ProblemDetails } from '../core/models';

@customElement('umb-installer-error')
export class UmbInstallerErrorElement extends LitElement {
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

	@property({ type: Object })
	error?: ProblemDetails;

	private _handleSubmit(e: SubmitEvent) {
		e.preventDefault();
		this.dispatchEvent(new CustomEvent('reset', { bubbles: true, composed: true }));
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

	render() {
		return html` <div id="container" class="uui-text" data-test="installer-error">
			<uui-form>
				<form id="installer-form" @submit="${this._handleSubmit}">
					<h1 class="uui-h3">Installing Umbraco</h1>
					<h2>Something went wrong</h2>
					${this.error ? this._renderError(this.error) : nothing}
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
