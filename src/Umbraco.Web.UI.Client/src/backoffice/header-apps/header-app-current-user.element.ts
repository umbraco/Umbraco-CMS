import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-header-app-current-user')
export class UmbHeaderAppCurrentUser extends LitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 14px;
			}
		`,
	];

	// TODO: Get current user information.

	render() {
		return html`
			<uui-button look="primary" label="My User Name" compact>
				<uui-avatar name="Extended Rabbit"></uui-avatar>
			</uui-button>
		`;
	}
}

export default UmbHeaderAppCurrentUser;

declare global {
	interface HTMLElementTagNameMap {
		'umb-header-app-current-user': UmbHeaderAppCurrentUser;
	}
}
