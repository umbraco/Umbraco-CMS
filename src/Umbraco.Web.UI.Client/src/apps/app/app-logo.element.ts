import { UMB_APP_CONTEXT } from './app.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-app-logo')
export class UmbAppLogoElement extends UmbLitElement {
	@state()
	private _logoUrl?: string;

	constructor() {
		super();

		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this._logoUrl = `${instance.getServerUrl()}/umbraco/management/api/v1/security/back-office/graphics/logo`;
		});
	}

	/**
	 * Do not use shadow DOM for this element.
	 * @returns {this} The element instance.
	 */
	override createRenderRoot(): this {
		return this;
	}

	override render() {
		if (!this._logoUrl) {
			return nothing;
		}

		return html`<img .src=${this._logoUrl} aria-hidden="true" loading="eager" alt="logo" style="height: 100%" />`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-logo': UmbAppLogoElement;
	}
}
