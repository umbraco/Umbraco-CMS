import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-document-sidebar-menu-item')
export class UmbDocumentSidebarMenuItemElement extends UmbLitElement {
	render() {
		return html`<umb-tree alias="Umb.Tree.Documents"></umb-tree>`;
	}
}

export default UmbDocumentSidebarMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-sidebar-menu-item': UmbDocumentSidebarMenuItemElement;
	}
}
