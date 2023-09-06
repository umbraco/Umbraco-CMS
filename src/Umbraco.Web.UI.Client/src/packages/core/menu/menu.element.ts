import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { ManifestMenu, ManifestMenuItem } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './menu-item/menu-item.element.js';

@customElement('umb-menu')
export class UmbMenuElement extends UmbLitElement {
	@property({ attribute: false })
	manifest?: ManifestMenu;

	constructor() {
		super();
		//this.provideContext(UMB_MENU_CONTEXT_TOKEN, new UmbMenuContext());
	}

	render() {
		return html` <umb-extension-slot
			type="menuItem"
			.filter=${(items: ManifestMenuItem) => items.meta.menus.includes(this.manifest!.alias)}
			default-element="umb-menu-item"></umb-extension-slot>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu': UmbMenuElement;
	}
}
