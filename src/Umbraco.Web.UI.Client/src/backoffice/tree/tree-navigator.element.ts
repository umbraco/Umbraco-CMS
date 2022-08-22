import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html``;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
