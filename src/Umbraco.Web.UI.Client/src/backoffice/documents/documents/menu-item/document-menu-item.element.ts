import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-document-menu-item')
export class UmbDocumentMenuItemElement extends UmbLitElement {
	render() {
		return html`<umb-tree alias="Umb.Tree.Documents"></umb-tree>`;
	}
}

export default UmbDocumentMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-menu-item': UmbDocumentMenuItemElement;
	}
}
