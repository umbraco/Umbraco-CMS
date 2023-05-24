import { css, CSSResultGroup, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbInstallerContext, UMB_INSTALLER_CONTEXT_TOKEN } from './installer.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './consent/installer-consent.element.js';
import './database/installer-database.element.js';
import './error/installer-error.element.js';
import './installing/installer-installing.element.js';
import './shared/layout/installer-layout.element.js';
import './user/installer-user.element.js';

@customElement('umb-installer')
export class UmbInstallerElement extends UmbLitElement {
	@state()
	step = 1;

	private _umbInstallerContext = new UmbInstallerContext();

	constructor() {
		super();
		this.provideContext(UMB_INSTALLER_CONTEXT_TOKEN, this._umbInstallerContext);
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._observeCurrentStep();
	}

	private _observeCurrentStep() {
		this.observe(this._umbInstallerContext.currentStepChanges(), (step) => {
			this.step = step;
		});

		this.observe(this._umbInstallerContext.installStatusChanges(), (error) => {
			if (error) {
				this.step = 5;
			}
		});
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

	static styles: CSSResultGroup = [css``];
}

export default UmbInstallerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer': UmbInstallerElement;
	}
}
