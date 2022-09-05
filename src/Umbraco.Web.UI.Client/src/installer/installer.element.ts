import './consent/installer-consent.element';
import './database/installer-database.element';
import './error/installer-error.element';
import './installing/installer-installing.element';
import './shared/layout/installer-layout.element';
import './user/installer-user.element';

import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { postInstallSetup } from '../core/api/fetcher';
import { UmbContextProviderMixin } from '../core/context';
import type { ProblemDetails } from '../core/models';
import { UmbInstallerContext } from './installer-context';

@customElement('umb-installer')
export class UmbInstallerElement extends UmbContextProviderMixin(LitElement) {
	static styles: CSSResultGroup = [css``];

	@state()
	step = 1;

	private _umbInstallerContext = new UmbInstallerContext();

	private _error?: ProblemDetails;

	constructor() {
		super();
		this.provideContext('umbInstallerContext', this._umbInstallerContext);
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.addEventListener('next', () => this._handleNext());
		this.addEventListener('previous', () => this._goToPreviousStep());
		this.addEventListener('submit', () => this._handleSubmit());
		this.addEventListener('reset', () => this._handleReset());
	}

	private _handleNext() {
		this.step++;
	}

	private _goToPreviousStep() {
		this.step--;
	}

	private _handleFulfilled() {
		console.warn('TODO: Set up real authentication');
		sessionStorage.setItem('is-authenticated', 'true');
		history.replaceState(null, '', '/content');
	}

	private _handleRejected(e: unknown) {
		if (e instanceof postInstallSetup.Error) {
			const error = e.getActualType();
			if (error.status === 400) {
				this._error = error.data;
			}
		}
		this._handleNext();
	}

	private _handleSubmit() {
		this._handleNext();

		this._umbInstallerContext
			.requestInstall()
			.then(() => this._handleFulfilled())
			.catch((error) => this._handleRejected(error));
	}

	private _handleReset() {
		this.step = 1;
		this._error = undefined;
	}

	private _renderSection() {
		switch (this.step) {
			case 2:
				return html`<umb-installer-consent></umb-installer-consent>`;
			case 3:
				return html`<umb-installer-database></umb-installer-database>`;
			case 4:
				return html`<umb-installer-installing></umb-installer-installing>`;
			case 5:
				return html`<umb-installer-error .error=${this._error}></umb-installer-error>`;

			default:
				return html`<umb-installer-user></umb-installer-user>`;
		}
	}

	render() {
		return html`<umb-installer-layout data-test="installer">${this._renderSection()}</umb-installer-layout> `;
	}
}

export default UmbInstallerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer': UmbInstallerElement;
	}
}
