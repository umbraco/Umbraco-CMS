import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-user-profile-app-profile')
export class UmbUserProfileAppProfileElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	private _edit() {
		//TODO implement me
	}

	private _changePassword() {
		//TODO implement me
	}

	render() {
		return html`
			<uui-box>
				<b slot="headline">Your profile - NOT IMPLEMENTED</b>
				<uui-button look="primary" @click=${this._edit}>Edit</uui-button>
				<uui-button look="primary" @click=${this._changePassword}>Change password</uui-button>
			</uui-box>
		`;
	}
}

export default UmbUserProfileAppProfileElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-profile-app-profile': UmbUserProfileAppProfileElement;
	}
}
