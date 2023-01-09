import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-dictionary-sidebar-menu-item')
export class UmbDictionarySidebarMenuItemElement extends UmbLitElement {
	render() {
		return html`<umb-tree alias="Umb.Tree.Dictionary"></umb-tree>`;
	}
}

export default UmbDictionarySidebarMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-sidebar-menu-item': UmbDictionarySidebarMenuItemElement;
	}
}
