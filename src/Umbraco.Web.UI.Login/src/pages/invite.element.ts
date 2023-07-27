import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-invite')
export default class UmbInviteElement extends LitElement {
	render() {
		return html`INVITE PAGE`;
	}

	static styles: CSSResultGroup = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-invite': UmbInviteElement;
	}
}
