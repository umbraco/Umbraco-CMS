import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';

import '../tree-navigator.element';
import '../tree-item.element';

@customElement('umb-tree-members')
export class UmbTreeMembers extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`Members Tree`;
	}
}

export default UmbTreeMembers;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-members': UmbTreeMembers;
	}
}
