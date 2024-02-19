import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-external-login-providers-user-profile-app')
export class UmbExternalLoginProvidersUserProfileAppElement extends UmbLitElement {
	render() {
		return html`
			<uui-box>
				<b slot="headline">External login providers</b>
				<umb-extension-slot id="externalLoginProviders" type="externalLoginProvider"></umb-extension-slot>
			</uui-box>
		`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbExternalLoginProvidersUserProfileAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-external-login-providers-user-profile-app': UmbExternalLoginProvidersUserProfileAppElement;
	}
}
