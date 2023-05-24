import { html } from '@umbraco-cms/backoffice/external/lit';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-dictionary-menu-item')
export class UmbDictionaryMenuItemElement extends UmbLitElement {
	render() {
		return html`<umb-tree alias="Umb.Tree.Dictionary" hide-tree-root></umb-tree>`;
	}
}

export default UmbDictionaryMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-menu-item': UmbDictionaryMenuItemElement;
	}
}
