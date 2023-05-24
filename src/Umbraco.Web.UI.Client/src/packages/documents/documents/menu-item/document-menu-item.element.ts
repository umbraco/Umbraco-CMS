import { html } from '@umbraco-cms/backoffice/external/lit';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-document-menu-item')
export class UmbDocumentMenuItemElement extends UmbLitElement {
	render() {
		return html`<umb-tree alias="Umb.Tree.Documents" hide-tree-root></umb-tree>`;
	}
}

export default UmbDocumentMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-menu-item': UmbDocumentMenuItemElement;
	}
}
