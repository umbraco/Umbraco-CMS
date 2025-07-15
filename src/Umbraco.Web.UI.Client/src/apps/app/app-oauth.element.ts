import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './app-error.element.js';

/**
 * A full page error element that can be used either solo or for instance as the error 500 page and BootFailed
 */
@customElement('umb-app-oauth')
export class UmbAppOauthElement extends UmbLitElement {
	/**
	 * Set to true if the login failed. A message will be shown instead of the loader.
	 * @attr
	 */
	@property({ type: Boolean })
	failure = false;

	override render() {
		// If we have a message, we show the error page
		// this is most likely happening inside a popup
		if (this.failure) {
			return html`<umb-app-error
				.errorHeadline=${this.localize.term('general_login')}
				.errorMessage=${this.localize.term('errors_externalLoginFailed')}
				hide-back-button></umb-app-error>`;
		}

		// If we don't have a message, we show the loader, this is most likely happening in the main app
		// for the normal login flow
		return html`
			<umb-body-layout id="loader" style="align-items:center;">
				<uui-loader></uui-loader>
			</umb-body-layout>
		`;
	}
}

export default UmbAppOauthElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-oauth': UmbAppOauthElement;
	}
}
