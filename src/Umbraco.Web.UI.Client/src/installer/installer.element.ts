import './consent/installer-consent.element';
import './database/installer-database.element';
import './error/installer-error.element';
import './installing/installer-installing.element';
import './shared/layout/installer-layout.element';
import './user/installer-user.element';

import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { Subscription } from 'rxjs';
import { UmbContextProviderMixin } from '../core/context';
import { UmbInstallerContext } from './installer.context';

@customElement('umb-installer')
export class UmbInstallerElement extends UmbContextProviderMixin(LitElement) {
	static styles: CSSResultGroup = [css``];

	@state()
	step = 1;

	private _umbInstallerContext = new UmbInstallerContext();

	private _currentStepSubscription?: Subscription;

	constructor() {
		super();
		this.provideContext('umbInstallerContext', this._umbInstallerContext);
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._observeCurrentStep();
	}

	private _observeCurrentStep() {
		this._currentStepSubscription?.unsubscribe();

		this._currentStepSubscription = this._umbInstallerContext
			.currentStepChanges()
			.subscribe((step) => (this.step = step));
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
				return html`<umb-installer-error></umb-installer-error>`;
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
