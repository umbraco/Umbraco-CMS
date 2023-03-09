import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-dictionary-menu-item')
export class UmbDictionaryMenuItemElement extends UmbLitElement {
	render() {
		return html`<umb-tree alias="Umb.Tree.Dictionary"></umb-tree>`;
	}
}

export default UmbDictionaryMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-menu-item': UmbDictionaryMenuItemElement;
	}
}
