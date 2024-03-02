import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestMenu, ManifestMenuItem } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './menu-item/menu-item-default.element.js';

@customElement('umb-menu')
export class UmbMenuElement extends UmbLitElement {
	@property({ attribute: false })
	manifest?: ManifestMenu;

	constructor() {
		super();
		//this.provideContext(UMB_MENU_CONTEXT, new UmbMenuContext());
	}

	render() {
		return html` <umb-extension-slot
			type="menuItem"
			.filter=${(items: ManifestMenuItem) => items.meta.menus.includes(this.manifest!.alias)}
			default-element="umb-menu-item-default"></umb-extension-slot>`;
	}
}

export default UmbMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu': UmbMenuElement;
	}
}
