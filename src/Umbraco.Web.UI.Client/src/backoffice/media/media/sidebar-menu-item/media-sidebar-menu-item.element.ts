import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/context-api';

@customElement('umb-media-sidebar-menu-item')
export class UmbMediaSidebarMenuItemElement extends UmbLitElement {
	render() {
		return html`<umb-tree alias="Umb.Tree.Media"></umb-tree>`;
	}
}

export default UmbMediaSidebarMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-sidebar-menu-item': UmbMediaSidebarMenuItemElement;
	}
}
