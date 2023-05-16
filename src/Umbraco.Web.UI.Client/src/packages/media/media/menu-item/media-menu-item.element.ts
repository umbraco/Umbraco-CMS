import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-media-menu-item')
export class UmbMediaMenuItemElement extends UmbLitElement {
	render() {
		return html`<umb-tree alias="Umb.Tree.Media" hide-tree-root></umb-tree>`;
	}
}

export default UmbMediaMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-menu-item': UmbMediaMenuItemElement;
	}
}
