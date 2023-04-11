import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-user-profile-app-external-login-providers')
export class UmbUserProfileAppExternalLoginProvidersElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`
			<uui-box>
				<b slot="headline">External login providers</b>
				<umb-extension-slot id="externalLoginProviders" type="externalLoginProvider"></umb-extension-slot>
			</uui-box>
		`;
	}
}

export default UmbUserProfileAppExternalLoginProvidersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-profile-app-external-login-providers': UmbUserProfileAppExternalLoginProvidersElement;
	}
}
