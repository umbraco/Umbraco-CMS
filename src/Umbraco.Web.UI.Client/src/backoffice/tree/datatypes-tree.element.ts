import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import './tree-navigator.element';
import './tree-item.element';

@customElement('umb-datatype-tree')
export class UmbDatatypeTree extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-datatype-tree': UmbDatatypeTree;
	}
}
