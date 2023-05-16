import { css, html, nothing, unsafeCSS } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';

import logoImg from '/umbraco_logomark_white.svg';
import backgroundImg from '/umbraco_background.jpg';

/**
 * A full page error element that can be used either solo or for instance as the error 500 page and BootFailed
 */
@customElement('umb-app-error')
export class UmbAppErrorElement extends UmbLitElement {
	/**
	 * The error message to display
	 *
	 * @attr
	 */
	@property()
	errorMessage?: string;

	/**
	 * The error to display
	 *
	 * @attr
	 */
	@property()
	error?: unknown;

	private renderProblemDetails = (problemDetails: ProblemDetailsModel) => html`
		<h2>${problemDetails.title}</h2>
		<p>${problemDetails.detail}</p>
		<pre>${problemDetails.stack}</pre>
	`;

	private renderErrorObj = (error: Error) => html`
		<h2>${error.name}</h2>
		<p>${error.message}</p>
		<pre>${error.stack}</pre>
	`;

	private isProblemDetails(error: unknown): error is ProblemDetailsModel {
		return typeof error === 'object' && error !== null && 'detail' in error && 'title' in error;
	}

	private isError(error: unknown): error is Error {
		return typeof error === 'object' && error !== null && error instanceof Error;
	}

	private renderError(error: unknown) {
		if (this.isProblemDetails(error)) {
			return this.renderProblemDetails(error);
		} else if (this.isError(error)) {
			return this.renderErrorObj(error);
		}

		return nothing;
	}

	render = () => html`
		<div id="background"></div>

		<div id="logo">
			<img src="${logoImg}" alt="Umbraco" />
		</div>

		<div id="container">
			<uui-box id="box">
				<h1>Something went wrong</h1>
				<p>${this.errorMessage}</p>
				${this.error
					? html`
							<details>
								<summary>Details</summary>
								${this.renderError(this.error)}
							</details>
					  `
					: nothing}
			</uui-box>
		</div>
	`;

	static styles = css`
		#background {
			position: fixed;
			overflow: hidden;
			background-position: 50%;
			background-repeat: no-repeat;
			background-size: cover;
			background-image: url('${unsafeCSS(backgroundImg)}');
			width: 100vw;
			height: 100vh;
		}

		#logo {
			position: fixed;
			top: var(--uui-size-space-5);
			left: var(--uui-size-space-5);
			height: 30px;
		}

		#logo img {
			height: 100%;
		}

		#container {
			position: relative;
			display: flex;
			align-items: center;
			justify-content: center;
			width: 100vw;
			height: 100vh;
		}

		#box {
			width: 50vw;
			padding: var(--uui-size-space-6) var(--uui-size-space-5) var(--uui-size-space-5) var(--uui-size-space-5);
		}

		details {
			padding: var(--uui-size-space-2) var(--uui-size-space-3);
			background: var(--uui-color-surface-alt);
		}

		pre {
			width: 100%;
			overflow: auto;
		}
	`;
}

export default UmbAppErrorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-error': UmbAppErrorElement;
	}
}
